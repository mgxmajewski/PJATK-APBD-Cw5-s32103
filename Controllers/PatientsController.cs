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
}