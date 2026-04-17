using MaintManager.Application.DTOs.Common;
using MaintManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

[ApiController]
[Route("api/v1/workers")]
[Authorize]
[Produces("application/json")]
public sealed class WorkersController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;

    public WorkersController(FleetMaintenanceContext context)
    {
        _context = context;
    }

    /// <summary>Obtener lista de técnicos disponibles (mecánicos activos).</summary>
    [HttpGet("technicians")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TechnicianDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnicians(CancellationToken ct)
    {
        var technicians = await _context.Workers
            .Include(w => w.Person)
            .Where(w => w.Status && !w.Locked)
            .Where(w => _context.Jobs
                .Where(j => j.Jobid == w.Jobid)
                .Select(j => j.Name)
                .FirstOrDefault()!
                .Contains("Mecánico"))
            .Select(w => new TechnicianDto(
                w.Workid,
                w.Person != null ? $"{w.Person.Fln} {w.Person.Name}".Trim() : w.Username ?? string.Empty
            ))
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<TechnicianDto>>.Ok(technicians));
    }
}

public sealed record TechnicianDto(int Workid, string FullName);
