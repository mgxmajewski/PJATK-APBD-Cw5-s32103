using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJATK_APBD_Cw5_s32103.Dtos;
using PJATK_APBD_Cw5_s32103.Models;

namespace PJATK_APBD_Cw5_s32103.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly HospitalDbContext _context;

    public PatientsController(HospitalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern));
        }

        var patients = await query
            .Select(p => new PatientDto(
                p.Pesel,
                p.FirstName,
                p.LastName,
                p.Age,
                p.Sex ? "Male" : "Female",
                p.Admissions
                    .Select(a => new AdmissionDto(
                        a.Id,
                        a.AdmissionDate,
                        a.DischargeDate,
                        new WardDto(a.Ward.Id, a.Ward.Name, a.Ward.Description)))
                    .ToList(),
                p.BedAssignments
                    .Select(ba => new BedAssignmentDto(
                        ba.Id,
                        ba.From,
                        ba.To,
                        new BedDto(
                            ba.Bed.Id,
                            new BedTypeDto(ba.Bed.BedType.Id, ba.Bed.BedType.Name, ba.Bed.BedType.Description),
                            new RoomDto(
                                ba.Bed.Room.Id,
                                ba.Bed.Room.HasTv,
                                new WardDto(ba.Bed.Room.Ward.Id, ba.Bed.Room.Ward.Name, ba.Bed.Room.Ward.Description)))))
                    .ToList()))
            .ToListAsync();

        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] CreateBedAssignmentDto dto)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
        if (!patientExists)
        {
            return NotFound($"Patient with PESEL '{pesel}' was not found.");
        }

        var ward = await _context.Wards.FirstOrDefaultAsync(w => w.Name == dto.Ward);
        if (ward is null)
        {
            return NotFound($"Ward '{dto.Ward}' was not found.");
        }

        var bedType = await _context.BedTypes.FirstOrDefaultAsync(bt => bt.Name == dto.BedType);
        if (bedType is null)
        {
            return NotFound($"Bed type '{dto.BedType}' was not found.");
        }

        if (dto.To.HasValue && dto.To.Value <= dto.From)
        {
            return BadRequest("'to' must be later than 'from'.");
        }

        var from = dto.From;
        var to = dto.To;

        var freeBed = await _context.Beds
            .Where(b => b.BedTypeId == bedType.Id && b.Room.WardId == ward.Id)
            .Where(b => !b.BedAssignments.Any(ba =>
                (to == null || ba.From < to) &&
                (ba.To == null || ba.To > from)))
            .FirstOrDefaultAsync();

        if (freeBed is null)
        {
            return NotFound(
                $"No free '{dto.BedType}' bed is available in ward '{dto.Ward}' for the requested period.");
        }

        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = freeBed.Id,
            From = from,
            To = to
        };

        _context.BedAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        var result = new
        {
            id = assignment.Id,
            patientPesel = assignment.PatientPesel,
            bedId = freeBed.Id,
            roomId = freeBed.RoomId,
            bedType = dto.BedType,
            ward = dto.Ward,
            from = assignment.From,
            to = assignment.To
        };

        return Created($"/api/patients/{pesel}/bedassignments/{assignment.Id}", result);
    }
}