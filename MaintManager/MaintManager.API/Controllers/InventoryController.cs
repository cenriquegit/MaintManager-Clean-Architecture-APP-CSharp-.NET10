using FluentValidation;
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Inventory;
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
using MaintManager.Domain.Entities;

namespace MaintManager.API.Controllers;

/// <summary>Gestión del inventario de materiales y lotes.</summary>
[ApiController]
[Route("api/v1/inventory")]
[Authorize]
[Produces("application/json")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IValidator<MaterialCreateRequest> _materialValidator;
    private readonly IValidator<LotCreateRequest> _lotValidator;
    private readonly FleetMaintenanceContext _context;

    private readonly IVehicleConfigRepository _vehicleConfigRepo;

    public InventoryController(
        IInventoryService inventoryService,
        IInventoryRepository inventoryRepo,
        IValidator<MaterialCreateRequest> materialValidator,
        IValidator<LotCreateRequest> lotValidator,
        FleetMaintenanceContext context,
        IVehicleConfigRepository vehicleConfigRepo)
    {
        _inventoryService = inventoryService;
        _inventoryRepo = inventoryRepo;
        _materialValidator = materialValidator;
        _lotValidator = lotValidator;
        _context = context;
        _vehicleConfigRepo = vehicleConfigRepo;
    }

    /// <summary>Obtener todos los materiales del inventario. Opcionalmente filtrado por vehículo.</summary>
    [HttpGet("materials")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaterialListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterials([FromQuery] int? vehicleId = null, CancellationToken ct = default)
    {
        IReadOnlyList<Material> materials;

        if (vehicleId.HasValue)
        {
            var allowed = await _vehicleConfigRepo.GetAllowedMaterialsAsync(vehicleId.Value, ct);
            if (allowed.Count > 0)
            {
                var allowedIds = allowed.Select(m => m.Mateid).ToHashSet();
                materials = await _context.Materials.AsNoTracking()
                    .Where(m => m.Status && allowedIds.Contains(m.Mateid))
                    .Include(m => m.Category)
                    .OrderBy(m => m.Category!.Name).ThenBy(m => m.Name)
                    .ToListAsync(ct);
            }
            else
            {
                materials = await _inventoryRepo.GetMaterialsAsync(ct);
            }
        }
        else
        {
            materials = await _inventoryRepo.GetMaterialsAsync(ct);
        }

        return Ok(ApiResponse<IReadOnlyList<MaterialListItem>>.Ok(
            materials.Select(m => m.ToListItem()).ToList()));
    }

    /// <summary>Obtener categorías de materiales.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _context.MaterialCategories
            .Where(c => c.Status)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryItem(c.Macaid, c.Name))
            .ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryItem>>.Ok(categories));
    }

    /// <summary>Obtener detalle de un material con sus lotes activos.</summary>
    [HttpGet("materials/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaterialResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterialById(int id, CancellationToken ct)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(id, ct);
        if (material is null)
            return NotFound(ApiResponse<object>.Fail(ErrorMessages.Inventory.MaterialNotFound));

        return Ok(ApiResponse<MaterialResponse>.Ok(material.ToResponse()));
    }

    /// <summary>Crear un nuevo material en inventario. Solo Admin.</summary>
    [HttpPost("materials")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMaterial(
        [FromBody] MaterialCreateRequest request, CancellationToken ct)
    {
        var validation = await _materialValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var createdBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _inventoryService.CreateMaterialAsync(
            request.Macaid, request.Name, request.UnitOfMeasure,
            request.StockMinimum, createdBy, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<object>.Ok(new { message = "Material creado correctamente." }));
    }

    /// <summary>Ingresar un nuevo lote de material.</summary>
    [HttpPost("materials/{id:int}/lots")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterLot(
        int id, [FromBody] LotCreateRequest request, CancellationToken ct)
    {
        var validation = await _lotValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var createdBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _inventoryService.RegisterLotAsync(
            id, request.Quantity, request.UnitCost, createdBy,
            request.ExpirationDate, request.Provid, request.SupplierLotNumber, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<object>.Ok(new { message = "Lote ingresado correctamente." }));
    }

    /// <summary>Obtener todos los lotes de un material, ordenados FIFO por vencimiento.</summary>
    [HttpGet("materials/{id:int}/lots")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LotResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLots(int id, CancellationToken ct)
    {
        var lots = await _inventoryRepo.GetFifoLotsAsync(id, ct);
        var result = lots.Select(l => l.ToResponse()).ToList();
        return Ok(ApiResponse<IReadOnlyList<LotResponse>>.Ok(result));
    }

    /// <summary>Descartar cantidad de un lote. Solo Admin.</summary>
    [HttpPost("lots/{lotId:int}/discard")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DiscardLot(
        int lotId, [FromBody] LotDiscardRequest request, CancellationToken ct)
    {
        var discardedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _inventoryService.DiscardLotAsync(
                lotId, request.Quantity, request.Reason, discardedBy, request.Note, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Descarte registrado correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Materiales con stock por debajo del mínimo.</summary>
    [HttpGet("materials/low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaterialListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        var materials = await _inventoryRepo.GetLowStockMaterialsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<MaterialListItem>>.Ok(
            materials.Select(m => m.ToListItem()).ToList()));
    }

    /// <summary>Lotes próximos a vencer (próximos 30 días por defecto).</summary>
    [HttpGet("expiring-lots")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LotResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringLots(
        [FromQuery] int days = 30, CancellationToken ct = default)
    {
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        var lots = await _inventoryRepo.GetExpiringLotsAsync(limitDate, ct);
        return Ok(ApiResponse<IReadOnlyList<LotResponse>>.Ok(
            lots.Select(l => l.ToResponse()).ToList()));
    }

    /// <summary>Calificar un material usado en un mantenimiento.</summary>
    [HttpPost("materials/{mateid:int}/ratings")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RateMaterial(
        int mateid, [FromBody] MaterialRatingRequest request, CancellationToken ct)
    {
        var ratedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _inventoryService.RateMaterialAsync(
                request.Mateid, request.Mainid, request.Rating, ratedBy, request.Observation, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Calificación registrada correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

public sealed record CategoryItem(short Macaid, string Name);

// ─────────────────────────────────────────────────────────────────────────────

