using FluentValidation;
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Maintenance;
using MaintManager.Shared.Models;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace MaintManager.API.Controllers;

/// <summary>Gestión de órdenes de mantenimiento vehicular.</summary>
[ApiController]
[Route("api/v1/maintenances")]
[Authorize]
[Produces("application/json")]
public sealed class MaintenancesController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly FleetMaintenanceContext _context;
    private readonly IValidator<MaintenanceCreateRequest> _createValidator;

    public MaintenancesController(
        IMaintenanceService maintenanceService,
        IMaintenanceRepository maintenanceRepo,
        IVehicleRepository vehicleRepo,
        FleetMaintenanceContext context,
        IValidator<MaintenanceCreateRequest> createValidator)
    {
        _maintenanceService = maintenanceService;
        _maintenanceRepo = maintenanceRepo;
        _vehicleRepo = vehicleRepo;
        _context = context;
        _createValidator = createValidator;
    }

    /// <summary>Obtener lista paginada de mantenimientos (optimizado sin N+1).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MaintenanceListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paged, CancellationToken ct)
    {
        var pagedResult = await _maintenanceRepo.GetPagedListItemsAsync(paged.Page, paged.PageSize, ct);
    
        var response = new PagedResponse<MaintenanceListItem>
        {
            Items = pagedResult.Items.Select(dto => new MaintenanceListItem(
                dto.Mainid,
                dto.LicensePlate,
                dto.VehicleName,
                dto.MaintenanceType,
                dto.ServiceType,
                dto.MaintenanceDate,
                dto.Mileage,
                dto.AssignedToName,
                dto.Status
            )).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return Ok(ApiResponse<PagedResponse<MaintenanceListItem>>.Ok(response));
    }

    /// <summary>Obtener detalle de una orden de mantenimiento.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var maintenance = await _maintenanceService.GetWithDetailsAsync(id, ct);
            var vehicle = await _vehicleRepo.GetByIdAsync(maintenance.Prcoid, ct);

            var assignedWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.AssignedTo, ct);
            var registeredWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.Workid, ct);

            return Ok(ApiResponse<MaintenanceResponse>.Ok(maintenance.ToResponse(
                vehicle?.LicensePlateNumber ?? string.Empty,
                vehicle?.Product?.Name ?? string.Empty,
                assignedWorker?.Person?.Name ?? string.Empty,
                registeredWorker?.Person?.Name ?? string.Empty)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Crear una nueva orden de mantenimiento.</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] MaintenanceCreateRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var registeredBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var assignedTo = request.AssignedTo > 0 ? request.AssignedTo : registeredBy;
            var maintenance = await _maintenanceService.CreateAsync(
                request.Prcoid, request.Matyid, request.Mileage,
                assignedTo, registeredBy, request.Setyid, request.Note, ct);

            return CreatedAtAction(nameof(GetById),
                new { id = maintenance.Mainid },
                ApiResponse<int>.Ok(maintenance.Mainid));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Completar una acción del checklist de la orden.</summary>
    [HttpPut("{id:int}/actions/{actionId:int}/complete")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteAction(
        int id, int actionId,
        [FromBody] ActionCompleteRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.CompleteActionAsync(
                actionId, request.ActionCode, request.ProductUsed,
                request.QuantityUsed, request.OriginProduct, request.Observation,
                request.Maloid, ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Acción completada correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Guardar el diagnóstico final del mecánico.</summary>
    [HttpPost("{id:int}/diagnosis")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveDiagnosis(
        int id,
        [FromBody] DiagnosisRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.SaveDiagnosisAsync(
                id, request.GeneralStatus, request.VehicleOperative,
                request.Observations, request.FutureRecommendations, ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Diagnóstico guardado correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Cerrar una orden de mantenimiento.</summary>
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(
        int id,
        [FromBody] MaintenanceCloseRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.CloseMaintenanceAsync(id, request.IsEmergencyComplete, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Orden cerrada correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Obtener todos los mantenimientos de un vehículo.</summary>
    [HttpGet("~/api/v1/vehicles/{vehicleId:int}/maintenances")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaintenanceListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVehicle(int vehicleId, CancellationToken ct)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId, ct);
        var maintenances = await _maintenanceService.GetByVehicleAsync(vehicleId, ct);
        var result = maintenances.Select(m => m.ToListItem(
            vehicle?.LicensePlateNumber ?? string.Empty,
            vehicle?.Product?.Name ?? string.Empty,
            string.Empty)).ToList();

        return Ok(ApiResponse<IReadOnlyList<MaintenanceListItem>>.Ok(result));
    }
}