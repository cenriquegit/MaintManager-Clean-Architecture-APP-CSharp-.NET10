using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Vehicle;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

/// <summary>Consulta de vehículos de la flota (solo lectura desde BD existente).</summary>
[ApiController]
[Route("api/v1/vehicles")]
[Authorize]
[Produces("application/json")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ISchedulingService _schedulingService;
    private readonly FleetMaintenanceContext _context;

    public VehiclesController(
        IVehicleRepository vehicleRepo,
        ISchedulingService schedulingService,
        FleetMaintenanceContext context)
    {
        _vehicleRepo = vehicleRepo;
        _schedulingService = schedulingService;
        _context = context;
    }

    /// <summary>Obtener lista de todos los vehículos activos de la flota.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VehicleListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var vehicles = await _vehicleRepo.GetActiveVehiclesAsync(ct);
        var result = new List<VehicleListItem>();

        foreach (var v in vehicles)
        {
            var currentKm = await _vehicleRepo.GetCurrentKmAsync(v.Prcoid, ct);
            var schedule = await _schedulingService.GetScheduleAsync(v.Prcoid, ct);
            var isDueSoon = await _schedulingService.IsMaintenanceDueSoonAsync(v.Prcoid, ct);
            result.Add(v.ToListItem(currentKm, schedule?.NextKm, isDueSoon));
        }

        return Ok(ApiResponse<IReadOnlyList<VehicleListItem>>.Ok(result));
    }

    /// <summary>Obtener detalle de un vehículo por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id, ct);
        if (vehicle is null)
            return NotFound(ApiResponse<object>.Fail(ErrorMessages.Vehicle.NotFound));

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        var schedule = await _schedulingService.GetScheduleAsync(id, ct);
        var isDueSoon = await _schedulingService.IsMaintenanceDueSoonAsync(id, ct);

        return Ok(ApiResponse<VehicleResponse>.Ok(
            vehicle.ToResponse(currentKm, schedule?.NextKm, schedule?.AlertKmThreshold, isDueSoon)));
    }

    /// <summary>Obtener el km actual de un vehículo.</summary>
    [HttpGet("{id:int}/current-km")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentKm(int id, CancellationToken ct)
    {
        var km = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        return Ok(ApiResponse<int>.Ok(km));
    }

    /// <summary>Obtener el cronograma de mantenimiento de un vehículo.</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedule(int id, CancellationToken ct)
    {
        var schedule = await _schedulingService.GetScheduleAsync(id, ct);
        if (schedule is null)
            return NotFound(ApiResponse<object>.Fail("Este vehículo no tiene cronograma configurado."));

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            schedule.Veshid,
            schedule.Prcoid,
            schedule.IntervalKm,
            schedule.NextKm,
            schedule.AlertKmThreshold,
            CurrentKm = currentKm,
            KmUntilService = schedule.NextKm - currentKm
        }));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

