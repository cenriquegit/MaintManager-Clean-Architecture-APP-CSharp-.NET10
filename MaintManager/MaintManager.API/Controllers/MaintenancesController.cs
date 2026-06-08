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
using MaintManager.Domain.Entities;
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
    private readonly IInventoryService _inventoryService;
    private readonly FleetMaintenanceContext _context;
    private readonly IValidator<MaintenanceCreateRequest> _createValidator;
    private readonly IVehicleConfigRepository _vehicleConfigRepository;

    public MaintenancesController(
        IMaintenanceService maintenanceService,
        IMaintenanceRepository maintenanceRepo,
        IVehicleRepository vehicleRepo,
        IInventoryService inventoryService,
        FleetMaintenanceContext context,
        IValidator<MaintenanceCreateRequest> createValidator,
        IVehicleConfigRepository vehicleConfigRepository)
    {
        _maintenanceService = maintenanceService;
        _maintenanceRepo = maintenanceRepo;
        _vehicleRepo = vehicleRepo;
        _inventoryService = inventoryService;
        _context = context;
        _createValidator = createValidator;
        _vehicleConfigRepository = vehicleConfigRepository;
    }

    /// <summary>Obtener lista paginada de mantenimientos (optimizado sin N+1).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MaintenanceListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paged, CancellationToken ct, [FromQuery] string? status = null)
    {
        var pagedResult = await _maintenanceRepo.GetPagedListItemsAsync(paged.Page, paged.PageSize, status, ct);
    
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

            var mv = await _context.ManagedVehicles.AsNoTracking()
                .FirstOrDefaultAsync(mv => mv.Prcoid == maintenance.Prcoid, ct);
            var mvId = mv?.MvId;
            var prcoid = maintenance.Prcoid;

            var actionsByPrcoid = await _vehicleConfigRepository.GetAllowedActionsAsync(prcoid, ct);
            var actionsByMvId = mvId.HasValue ? await _vehicleConfigRepository.GetAllowedActionsByMvIdAsync(mvId.Value, ct) : new List<Domain.Entities.VehicleAllowedAction>();
            var materialsByPrcoid = await _vehicleConfigRepository.GetAllowedMaterialsAsync(prcoid, ct);
            var materialsByMvId = mvId.HasValue ? await _vehicleConfigRepository.GetAllowedMaterialsByMvIdAsync(mvId.Value, ct) : new List<Domain.Entities.VehicleAllowedMaterial>();
            var componentsByPrcoid = await _vehicleConfigRepository.GetAllowedComponentsAsync(prcoid, ct);
            var componentsByMvId = mvId.HasValue ? await _vehicleConfigRepository.GetAllowedComponentsByMvIdAsync(mvId.Value, ct) : new List<Domain.Entities.VehicleAllowedComponent>();

            var allActionIds = actionsByPrcoid.Select(a => a.Acatid)
                .Union(actionsByMvId.Select(a => a.Acatid)).Distinct().ToList();
            var allMaterialIds = materialsByPrcoid.Select(m => m.Mateid)
                .Union(materialsByMvId.Select(m => m.Mateid)).Distinct().ToList();
            var allComponentIds = componentsByPrcoid.Select(c => c.Acatid)
                .Union(componentsByMvId.Select(c => c.Acatid)).Distinct().ToList();

            var mergedActionIds = actionsByPrcoid.Select(a => a.Acatid)
                .Union(actionsByMvId.Select(a => a.Acatid)).Distinct().ToList();
            var mergedMaterialIds = materialsByPrcoid.Select(m => m.Mateid)
                .Union(materialsByMvId.Select(m => m.Mateid)).Distinct().ToList();
            var mergedComponentIds = componentsByPrcoid.Select(c => c.Acatid)
                .Union(componentsByMvId.Select(c => c.Acatid)).Distinct().ToList();

            var assignedWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.AssignedTo, ct);
            var registeredWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.Workid, ct);

            return Ok(ApiResponse<MaintenanceResponse>.Ok(maintenance.ToResponse(
                vehicle?.LicensePlateNumber ?? string.Empty,
                vehicle?.Product?.Name ?? string.Empty,
                assignedWorker?.Person?.Name ?? string.Empty,
                registeredWorker?.Person?.Name ?? string.Empty,
                mergedActionIds,
                mergedMaterialIds,
                mergedComponentIds)));
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
    [HttpPost("{id:int}/actions")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAction(
        int id, [FromBody] AddActionRequest request,
        CancellationToken ct)
    {
        try
        {
            var madeid = await _maintenanceService.CreateActionAsync(id, request.ActionCatalogId, ct);
            return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<int>.Ok(madeid));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

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

    /// <summary>Consumir material de inventario (FIFO) en una orden de mantenimiento.</summary>
    [HttpPost("{id:int}/consume")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsumeMaterial(
        int id,
        [FromBody] ConsumeRequest request,
        CancellationToken ct)
    {
        try
        {
            await _inventoryService.ConsumeStockFifoAsync(
                request.Mateid, request.Quantity, id, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Material consumido correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Instalar un componente en el vehículo durante un mantenimiento.</summary>
    [HttpPost("{id:int}/components")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> InstallComponent(
        int id,
        [FromBody] InstallComponentRequest request,
        CancellationToken ct)
    {
        try
        {
            var maintenance = await _maintenanceRepo.GetWithDetailsAsync(id, ct);
            var component = InstalledComponent.Create(
                maintenance.Prcoid, request.ActionCatalogId, id,
                maintenance.Mileage, request.LotId, request.UsefulLifeDays);

            _context.InstalledComponents.Add(component);
            await _context.SaveChangesAsync(ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Componente instalado correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/actions/{madeid:int}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAction(int id, int madeid, CancellationToken ct)
    {
        try
        {
            var maintenance = await _context.Maintenances
                .Include(m => m.ActionDetails)
                .FirstOrDefaultAsync(m => m.Mainid == id, ct);
            if (maintenance is null)
                return NotFound(ApiResponse<object>.Fail("Orden no encontrada."));
            if (maintenance.Statid == "FI")
                return BadRequest(ApiResponse<object>.Fail("No se puede modificar una orden finalizada."));

            var detail = maintenance.ActionDetails.FirstOrDefault(d => d.Madeid == madeid);
            if (detail is null)
                return NotFound(ApiResponse<object>.Fail("Acción no encontrada."));

            _context.Remove(detail);
            await _context.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Acción eliminada." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/materials/{macoid:int}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMaterialConsumption(int id, int macoid, CancellationToken ct)
    {
        try
        {
            var consumption = await _context.MaterialConsumptions
                .FirstOrDefaultAsync(c => c.Macoid == macoid && c.Mainid == id, ct);
            if (consumption is null)
                return NotFound(ApiResponse<object>.Fail("Consumo no encontrado."));

            var maintenance = await _context.Maintenances.FindAsync(new object[] { id }, ct);
            if (maintenance?.Statid == "FI")
                return BadRequest(ApiResponse<object>.Fail("No se puede modificar una orden finalizada."));

            _context.Remove(consumption);
            await _context.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Consumo eliminado." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/components/{incoid:int}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteComponent(int id, int incoid, CancellationToken ct)
    {
        try
        {
            var component = await _context.InstalledComponents
                .FirstOrDefaultAsync(c => c.Incoid == incoid && c.Mainid == id, ct);
            if (component is null)
                return NotFound(ApiResponse<object>.Fail("Componente no encontrado."));

            var maintenance = await _context.Maintenances.FindAsync(new object[] { id }, ct);
            if (maintenance?.Statid == "FI")
                return BadRequest(ApiResponse<object>.Fail("No se puede modificar una orden finalizada."));

            _context.Remove(component);
            await _context.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Componente eliminado." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Reasignar técnico responsable de la orden. Solo Admin.</summary>
    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignTechnician(
        int id,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken ct)
    {
        try
        {
            var maintenance = await _maintenanceRepo.GetWithDetailsAsync(id, ct);
            if (maintenance is null)
                return NotFound(ApiResponse<object>.Fail("Orden no encontrada."));

            maintenance.AssignTo(request.Workid);
            await _maintenanceRepo.SaveChangesAsync(ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Técnico reasignado correctamente." }));
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

    /// <summary>Cancelar una orden de mantenimiento activa.</summary>
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        try
        {
            var maintenance = await _context.Maintenances.FindAsync(new object[] { id }, ct);
            if (maintenance is null)
                return NotFound(ApiResponse<object>.Fail("Orden no encontrada."));

            if (maintenance.Statid != "AC")
                return BadRequest(ApiResponse<object>.Fail("Solo se pueden cancelar órdenes activas."));

            maintenance.MarkAsCancelled();
            await _context.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Orden cancelada correctamente." }));
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

    /// <summary>Estadísticas rápidas de mantenimientos para el panel principal.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceStatsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalFi = await _context.Maintenances
            .CountAsync(m => m.Statid == "FI", ct);
        var totalAc = await _context.Maintenances
            .CountAsync(m => m.Statid == "AC", ct);
        var completedThisMonth = await _context.Maintenances
            .CountAsync(m => m.Statid == "FI" && m.MaintenanceDate >= monthStart, ct);
        var emergencyThisMonth = await _context.Maintenances
            .CountAsync(m => m.Matyid == 2
                && m.MaintenanceDate >= monthStart, ct);

        return Ok(ApiResponse<MaintenanceStatsResponse>.Ok(
            new MaintenanceStatsResponse(totalFi, totalAc, completedThisMonth, emergencyThisMonth)));
    }

    /// <summary>Catálogo de acciones disponibles. Opcionalmente filtrado por vehículo.</summary>
    [HttpGet("actions/catalog")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ActionCatalogItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActionCatalog([FromQuery] int? vehicleId = null, CancellationToken ct = default)
    {
        if (vehicleId.HasValue && vehicleId.Value > 0)
        {
            var allowedActions = await _vehicleConfigRepository.GetAllowedActionsAsync(vehicleId.Value, ct);
            var allowedComponents = await _vehicleConfigRepository.GetAllowedComponentsAsync(vehicleId.Value, ct);

            if (allowedActions.Count > 0 || allowedComponents.Count > 0)
            {
                var allowedActionIds = allowedActions.Select(a => a.Acatid).ToHashSet();
                var allowedComponentIds = allowedComponents.Select(c => c.Acatid).ToHashSet();

                var all = await _context.ActionCatalogs.Where(a => a.Status)
                    .OrderBy(a => a.Category).ThenBy(a => a.Name)
                    .Select(a => new ActionCatalogItem(a.Acatid, a.Name, a.Category))
                    .ToListAsync(ct);

                var filtered = all.Where(a =>
                    (a.Category == null || !a.Category.Contains("Componente") ? allowedActionIds.Contains(a.Acatid) : allowedComponentIds.Contains(a.Acatid))
                ).ToList();

                return Ok(ApiResponse<IReadOnlyList<ActionCatalogItem>>.Ok(filtered));
            }
        }

        var items = await _context.ActionCatalogs.Where(a => a.Status)
            .OrderBy(a => a.Category).ThenBy(a => a.Name)
            .Select(a => new ActionCatalogItem(a.Acatid, a.Name, a.Category))
            .ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ActionCatalogItem>>.Ok(items));
    }
}

public sealed record MaintenanceStatsResponse(
    int Scheduled,
    int InProgress,
    int CompletedThisMonth,
    int EmergencyThisMonth
);

public sealed record ActionCatalogItem(int Acatid, string Name, string? Category);