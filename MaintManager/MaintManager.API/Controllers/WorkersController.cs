using System.Security.Cryptography;
using System.Text;
using MaintManager.Application.DTOs.Common;
using MaintManager.Domain.Entities.Existing;
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

    /// <summary>Crear un nuevo trabajador (solo Admin).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorker([FromBody] CreateWorkerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<object>.Fail("Usuario y contraseña requeridos."));

        var existing = await _context.Workers
            .AnyAsync(w => w.Username == request.Username, ct);
        if (existing)
            return BadRequest(ApiResponse<object>.Fail("El nombre de usuario ya existe."));

        short? jobId = null;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var job = await _context.Jobs
                .Where(j => j.Name == request.Role)
                .FirstOrDefaultAsync(ct);
            if (job is not null) jobId = job.Jobid;
        }

        var person = new Person
        {
            Name = request.Name ?? string.Empty,
            Fln = request.Fln ?? string.Empty,
            Mln = request.Mln
        };
        _context.Persons.Add(person);
        await _context.SaveChangesAsync(ct);

        var worker = new Worker
        {
            Persid = person.Persid,
            Jobid = jobId ?? 1,
            Username = request.Username,
            Password = ComputeMd5(request.Password),
            Email = request.Email,
            Status = true,
            Locked = false
        };
        _context.Workers.Add(worker);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetTechnicians), new { }, ApiResponse<object>.Ok(new { worker.Workid }));
    }

    private static string ComputeMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record TechnicianDto(int Workid, string FullName);

public sealed record CreateWorkerRequest(
    string Username,
    string Password,
    string? Name,
    string? Fln,
    string? Mln,
    string? Email,
    string? Role);
