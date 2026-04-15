
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de gestión de inventario.</summary>
public interface IInventoryService
{
    Task CreateMaterialAsync(short macaid, string name, string unit, decimal stockMin, int createdBy, CancellationToken ct = default);
    Task RegisterLotAsync(int mateid, decimal quantity, decimal unitCost, int createdBy,
        DateOnly? expirationDate, int? provid, string? supplierLot, CancellationToken ct = default);

    /// <summary>
    /// Consume material de los lotes usando FIFO por fecha de vencimiento.
    /// Si el consumo abarca más de un lote, genera múltiples registros de MaterialConsumption.
    /// </summary>
    Task<IReadOnlyList<int>> ConsumeStockFifoAsync(int mateid, decimal quantity,
        int mainid, CancellationToken ct = default);

    Task DiscardLotAsync(int maloid, decimal quantity, string reason,
        int discardedBy, string? note, CancellationToken ct = default);
    Task RateMaterialAsync(int mateid, int mainid, short rating, int ratedBy, string? observation, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

