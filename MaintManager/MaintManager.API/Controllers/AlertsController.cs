using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaintManager.API.Controllers;

/// <summary>Gestión del sistema de alertas.</summary>
[ApiController]
[Route("api/v1/alerts")]
[Authorize]
[Produces("application/json")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepo;
    private readonly IAlertService _alertService;

    public AlertsController(IAlertRepository alertRepo, IAlertService alertService)
    {
        _alertRepo = alertRepo;
        _alertService = alertService;
    }

    /// <summary>Obtener alertas no resueltas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnresolved(CancellationToken ct)
    {
        var alerts = await _alertRepo.GetUnresolvedAlertsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<AlertResponse>>.Ok(
            alerts.Select(a => a.ToAlertResponse()).ToList()));
    }

    /// <summary>Marcar alerta como leída.</summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var workid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _alertService.MarkAsReadAsync(id, workid, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Alerta marcada como leída." }));
    }

    /// <summary>Resolver una alerta. Solo Admin.</summary>
    [HttpPut("{id:int}/resolve")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(int id, CancellationToken ct)
    {
        var workid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _alertService.ResolveAsync(id, workid, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Alerta resuelta correctamente." }));
    }

    /// <summary>Ejecutar verificación de alertas manualmente.</summary>
    [HttpPost("check")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAlerts(CancellationToken ct)
    {
        await _alertService.CheckAndGenerateAlertsAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Verificación completada." }));
    }
}
