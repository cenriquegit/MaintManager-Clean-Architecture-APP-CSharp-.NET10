// MaintManager.Application/Services/InventoryService.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.Application.Services;

/// <summary>Gestión de inventario de materiales con FIFO por vencimiento.</summary>
public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;

    public InventoryService(IInventoryRepository inventoryRepo) =>
        _inventoryRepo = inventoryRepo;

    public async Task CreateMaterialAsync(
        short macaid, string name, string unit, decimal stockMin,
        int createdBy, CancellationToken ct = default)
    {
        var material = Material.Create(macaid, name, unit, stockMin, createdBy);
        await _inventoryRepo.AddMaterialAsync(material, ct);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task RegisterLotAsync(
        int mateid, decimal quantity, decimal unitCost, int createdBy,
        DateOnly? expirationDate, int? provid, string? supplierLot,
        CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        var lot = MaterialLot.Create(mateid, quantity, unitCost, createdBy,
            expirationDate, provid, supplierLot);

        await _inventoryRepo.AddLotAsync(lot, ct);
        material.AddStock(quantity);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<int>> ConsumeStockFifoAsync(
        int mateid, decimal quantity, int mainid, CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        if (material.StockTotal < quantity)
            throw new InvalidOperationException(ErrorMessages.Inventory.InsufficientStock);

        var lots = await _inventoryRepo.GetFifoLotsAsync(mateid, ct);
        var consumedLotIds = new List<int>();
        var remaining = quantity;

        foreach (var lot in lots)
        {
            if (remaining <= 0) break;

            var toConsume = Math.Min(remaining, lot.CurrentQuantity);
            lot.Consume(toConsume);
            _inventoryRepo.UpdateLot(lot);

            var consumption = MaterialConsumption.Create(mainid, mateid, toConsume, "Stock propio", lot.Maloid);
            await _inventoryRepo.AddConsumptionAsync(consumption, ct);
            consumedLotIds.Add(lot.Maloid);
            remaining -= toConsume;
        }

        material.DeductStock(quantity);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);

        return consumedLotIds;
    }

    public async Task DiscardLotAsync(
        int maloid, decimal quantity, string reason, int discardedBy,
        string? note, CancellationToken ct = default)
    {
        var lot = await _inventoryRepo.GetLotByIdAsync(maloid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.LotNotFound);

        if (lot.LotStatus == "vencido" || lot.LotStatus == "descartado")
            throw new InvalidOperationException(ErrorMessages.Inventory.LotExpired);

        var material = await _inventoryRepo.GetMaterialByIdAsync(lot.Mateid, ct)!;
        lot.Discard(quantity);
        material!.DeductStock(quantity);

        var discard = MaterialDiscard.Create(maloid, quantity, reason, discardedBy, note);
        await _inventoryRepo.AddDiscardAsync(discard, ct);
        _inventoryRepo.UpdateLot(lot);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task RateMaterialAsync(
        int mateid, int mainid, short rating, int ratedBy,
        string? observation, CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        var materialRating = MaterialRating.Create(mateid, mainid, rating, ratedBy, observation);
        // Se agrega via el contexto — EF Core lo maneja con la relación
        await _inventoryRepo.SaveChangesAsync(ct);
    }
}

