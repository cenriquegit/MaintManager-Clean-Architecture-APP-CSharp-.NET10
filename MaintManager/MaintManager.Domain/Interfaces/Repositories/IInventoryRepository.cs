
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio del inventario de materiales y lotes.</summary>
public interface IInventoryRepository
{
    // Materiales
    Task<Material?> GetMaterialByIdAsync(int mateid, CancellationToken ct = default);
    Task<IReadOnlyList<Material>> GetMaterialsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Material>> GetLowStockMaterialsAsync(CancellationToken ct = default);
    Task AddMaterialAsync(Material material, CancellationToken ct = default);
    void UpdateMaterial(Material material);

    // Lotes
    Task<MaterialLot?> GetLotByIdAsync(int maloid, CancellationToken ct = default);
    Task<IReadOnlyList<MaterialLot>> GetActiveLotsByMaterialAsync(int mateid, CancellationToken ct = default);

    /// <summary>
    /// Devuelve los lotes activos de un material ordenados FIFO por vencimiento.
    /// Los lotes sin fecha de vencimiento van al final.
    /// </summary>
    Task<IReadOnlyList<MaterialLot>> GetFifoLotsAsync(int mateid, CancellationToken ct = default);

    Task<IReadOnlyList<MaterialLot>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default);
    Task AddLotAsync(MaterialLot lot, CancellationToken ct = default);
    void UpdateLot(MaterialLot lot);

    // Consumos y descartes
    Task AddConsumptionAsync(MaterialConsumption consumption, CancellationToken ct = default);
    Task AddDiscardAsync(MaterialDiscard discard, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

