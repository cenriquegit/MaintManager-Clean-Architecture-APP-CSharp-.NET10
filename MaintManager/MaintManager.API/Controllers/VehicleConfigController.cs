using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Vehicle;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

[ApiController]
[Route("api/v1/vehicles/{vehicleId:int}/config")]
[Authorize(Roles = RoleNames.Admin)]
[Produces("application/json")]
public sealed class VehicleConfigController : ControllerBase
{
    private readonly IVehicleConfigRepository _configRepo;
    private readonly IVehicleConfigService _configService;
    private readonly FleetMaintenanceContext _context;
    private readonly IManagedVehicleRepository _mvRepo;

    public VehicleConfigController(
        IVehicleConfigRepository configRepo,
        IVehicleConfigService configService,
        FleetMaintenanceContext context,
        IManagedVehicleRepository mvRepo)
    {
        _configRepo = configRepo;
        _configService = configService;
        _context = context;
        _mvRepo = mvRepo;
    }

    private async Task<bool> VehicleExistsAsync(int vehicleId, int? mvId, CancellationToken ct) =>
        mvId.HasValue
            ? await _mvRepo.GetByIdAsync(mvId.Value, ct) is not null
            : await _context.Vehicles.AnyAsync(v => v.Prcoid == vehicleId, ct);

    private (int? prcoid, int? mvId) Resolve(int vehicleId, int? mvId) =>
        mvId.HasValue ? (null, mvId) : (vehicleId, null);

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<VehicleConfigResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfig(int vehicleId, [FromQuery] int? mvId, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        var actions = mid.HasValue
            ? await _configRepo.GetAllowedActionsByMvIdAsync(mid.Value, ct)
            : await _configRepo.GetAllowedActionsAsync(vehicleId, ct);
        var materials = mid.HasValue
            ? await _configRepo.GetAllowedMaterialsByMvIdAsync(mid.Value, ct)
            : await _configRepo.GetAllowedMaterialsAsync(vehicleId, ct);
        var components = mid.HasValue
            ? await _configRepo.GetAllowedComponentsByMvIdAsync(mid.Value, ct)
            : await _configRepo.GetAllowedComponentsAsync(vehicleId, ct);

        var catalog = await _context.ActionCatalogs.AsNoTracking().ToListAsync(ct);
        var allMaterials = await _context.Materials.AsNoTracking().ToListAsync(ct);

        var response = new VehicleConfigResponse(
            mid ?? vehicleId,
            actions.Select(a =>
            {
                var cat = catalog.FirstOrDefault(c => c.Acatid == a.Acatid);
                return new ActionConfigItem(a.Acatid, cat?.Name ?? "?", cat?.Category);
            }).ToList(),
            materials.Select(m =>
            {
                var mat = allMaterials.FirstOrDefault(mt => mt.Mateid == m.Mateid);
                return new MaterialConfigItem(m.Mateid, mat?.Name ?? "?", mat?.UnitOfMeasure);
            }).ToList(),
            components.Select(c =>
            {
                var cat = catalog.FirstOrDefault(ca => ca.Acatid == c.Acatid);
                return new ActionConfigItem(c.Acatid, cat?.Name ?? "?", cat?.Category);
            }).ToList()
        );

        return Ok(ApiResponse<VehicleConfigResponse>.Ok(response));
    }

    [HttpPost("actions")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddAction(int vehicleId, [FromQuery] int? mvId, [FromBody] AddActionConfigRequest request, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));
        if (!await _context.ActionCatalogs.AnyAsync(a => a.Acatid == request.Acatid && a.Status, ct))
            return BadRequest(ApiResponse<object>.Fail("Acción no encontrada."));

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        var existing = await _context.Set<Domain.Entities.VehicleAllowedAction>()
            .AnyAsync(v => (mid.HasValue && v.MvId == mid.Value || prcoid.HasValue && v.Prcoid == prcoid.Value)
                && v.Acatid == request.Acatid, ct);
        if (existing)
            return Ok(ApiResponse<object>.Ok(new { message = "Acción ya estaba asociada." }));

        await _configRepo.AddAllowedActionAsync(prcoid, request.Acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Acción agregada." }));
    }

    [HttpDelete("actions/{acatid:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveAction(int vehicleId, int acatid, [FromQuery] int? mvId, CancellationToken ct)
    {
        var (prcoid, mid) = Resolve(vehicleId, mvId);
        await _configRepo.RemoveAllowedActionAsync(prcoid, acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Acción eliminada." }));
    }

    [HttpPost("materials")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMaterial(int vehicleId, [FromQuery] int? mvId, [FromBody] AddMaterialConfigRequest request, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));
        if (!await _context.Materials.AnyAsync(m => m.Mateid == request.Mateid && m.Status, ct))
            return BadRequest(ApiResponse<object>.Fail("Material no encontrado."));

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        var existing = await _context.Set<Domain.Entities.VehicleAllowedMaterial>()
            .AnyAsync(v => (mid.HasValue && v.MvId == mid.Value || prcoid.HasValue && v.Prcoid == prcoid.Value)
                && v.Mateid == request.Mateid, ct);
        if (existing)
            return Ok(ApiResponse<object>.Ok(new { message = "Material ya estaba asociado." }));

        await _configRepo.AddAllowedMaterialAsync(prcoid, request.Mateid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Material agregado." }));
    }

    [HttpDelete("materials/{mateid:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveMaterial(int vehicleId, int mateid, [FromQuery] int? mvId, CancellationToken ct)
    {
        var (prcoid, mid) = Resolve(vehicleId, mvId);
        await _configRepo.RemoveAllowedMaterialAsync(prcoid, mateid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Material eliminado." }));
    }

    [HttpPost("components")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddComponent(int vehicleId, [FromQuery] int? mvId, [FromBody] AddActionConfigRequest request, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));
        if (!await _context.ActionCatalogs.AnyAsync(a => a.Acatid == request.Acatid && a.Status && a.Category != null && a.Category.Contains("Componente"), ct))
            return BadRequest(ApiResponse<object>.Fail("Componente no encontrado."));

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        var existing = await _context.Set<Domain.Entities.VehicleAllowedComponent>()
            .AnyAsync(v => (mid.HasValue && v.MvId == mid.Value || prcoid.HasValue && v.Prcoid == prcoid.Value)
                && v.Acatid == request.Acatid, ct);
        if (existing)
            return Ok(ApiResponse<object>.Ok(new { message = "Componente ya estaba asociado." }));

        await _configRepo.AddAllowedComponentAsync(prcoid, request.Acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Componente agregado." }));
    }

    [HttpDelete("components/{acatid:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveComponent(int vehicleId, int acatid, [FromQuery] int? mvId, CancellationToken ct)
    {
        var (prcoid, mid) = Resolve(vehicleId, mvId);
        await _configRepo.RemoveAllowedComponentAsync(prcoid, acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Componente eliminado." }));
    }

    [HttpPost("actions/create")]
    [ProducesResponseType(typeof(ApiResponse<ActionConfigItem>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateActionAndAdd(int vehicleId, [FromQuery] int? mvId, [FromBody] CreateActionConfigRequest request, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        var catalog = await _configService.CreateActionCatalogAsync(
            request.Name, request.Category, request.Description, request.UsefulLifeDays, ct);

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        await _configRepo.AddAllowedActionAsync(prcoid, catalog.Acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetConfig), new { vehicleId },
            ApiResponse<ActionConfigItem>.Ok(new ActionConfigItem(catalog.Acatid, catalog.Name, catalog.Category)));
    }

    [HttpPost("components/create")]
    [ProducesResponseType(typeof(ApiResponse<ActionConfigItem>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateComponentAndAdd(int vehicleId, [FromQuery] int? mvId, [FromBody] CreateActionConfigRequest request, CancellationToken ct)
    {
        if (!await VehicleExistsAsync(vehicleId, mvId, ct))
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        var catalog = await _configService.CreateActionCatalogAsync(
            request.Name, "Componente", request.Description, request.UsefulLifeDays, ct);

        var (prcoid, mid) = Resolve(vehicleId, mvId);
        await _configRepo.AddAllowedComponentAsync(prcoid, catalog.Acatid, mid, ct);
        await _configRepo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetConfig), new { vehicleId },
            ApiResponse<ActionConfigItem>.Ok(new ActionConfigItem(catalog.Acatid, catalog.Name, catalog.Category)));
    }
}

public sealed record AddActionConfigRequest(int Acatid);
public sealed record AddMaterialConfigRequest(int Mateid);
public sealed record CreateActionConfigRequest(string Name, string? Category, string? Description, int? UsefulLifeDays);
