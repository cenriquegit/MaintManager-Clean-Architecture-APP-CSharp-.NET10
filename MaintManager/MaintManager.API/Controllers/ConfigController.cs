using MaintManager.Application.DTOs.Common;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MaintManager.API.Controllers;

[ApiController]
[Route("api/v1/config")]
[Authorize(Roles = RoleNames.Admin)]
[Produces("application/json")]
public sealed class ConfigController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;

    public ConfigController(FleetMaintenanceContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ConfigItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var configs = await _context.ConfigSystems
            .Where(c => c.Status)
            .OrderBy(c => c.Key)
            .Select(c => new ConfigItem(c.Key, c.Value, c.Description ?? string.Empty))
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<ConfigItem>>.Ok(configs));
    }

    [HttpPut("{key}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateConfigRequest request, CancellationToken ct)
    {
        var config = await _context.ConfigSystems.FirstOrDefaultAsync(c => c.Key == key, ct);
        if (config is null)
            return NotFound(ApiResponse<object>.Fail($"Parámetro '{key}' no encontrado."));

        var workid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _context.ConfigSystems
            .Where(c => c.Key == key)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Value, request.Value)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow)
                .SetProperty(c => c.UpdatedBy, workid), ct);

        return Ok(ApiResponse<object>.Ok(new { message = "Configuración actualizada." }));
    }
}

public sealed record ConfigItem(string Key, string Value, string Description);
public sealed record UpdateConfigRequest(string Value);
