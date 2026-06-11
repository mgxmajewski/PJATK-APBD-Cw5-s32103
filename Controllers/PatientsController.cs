using PJATK_APBD_Cw5_s32103.Models;

namespace PJATK_APBD_Cw5_s32103.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



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
    public async Task<IActionResult> GetAllPatients()
    {
        var patients = await _context.Patients.ToListAsync();
        return Ok(patients);
    }
}