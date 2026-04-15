// MaintManager.Infrastructure/Repositories/AllRepositories.cs
// ACTUALIZADO:
// — VehicleRepository: GetCurrentKmAsync corregido con nueva estructura BD-FINAL
//   (join rentrequest→prodid para obtener los km del vehículo)
// — El resto de repositorios no cambia respecto a la versión anterior
using MaintManager.Domain.Entities;
using MaintManager.Domain.Entities.Existing;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;
{

internal sealed class InventoryRepository : IInventoryRepository
{
    private readonly FleetMaintenanceContext _context;

    public InventoryRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<Material?> GetMaterialByIdAsync(int mateid, CancellationToken ct = default) =>
        await _context.Materials.Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Mateid == mateid, ct);

    public async Task<IReadOnlyList<Material>> GetMaterialsAsync(CancellationToken ct = default) =>
        await _context.Materials.AsNoTracking()
            .Where(m => m.Status)
            .Include(m => m.Category)
            .OrderBy(m => m.Category!.Name).ThenBy(m => m.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Material>> GetLowStockMaterialsAsync(CancellationToken ct = default) =>
        await _context.Materials.AsNoTracking()
            .Where(m => m.Status && m.StockTotal < m.StockMinimum)
            .Include(m => m.Category)
            .ToListAsync(ct);

    public async Task AddMaterialAsync(Material material, CancellationToken ct = default) =>
        await _context.Materials.AddAsync(material, ct);

    public void UpdateMaterial(Material material) => _context.Materials.Update(material);

    public async Task<MaterialLot?> GetLotByIdAsync(int maloid, CancellationToken ct = default) =>
        await _context.MaterialLots.Include(ml => ml.Material)
            .FirstOrDefaultAsync(ml => ml.Maloid == maloid, ct);

    public async Task<IReadOnlyList<MaterialLot>> GetActiveLotsByMaterialAsync(int mateid, CancellationToken ct = default) =>
        await _context.MaterialLots.AsNoTracking()
            .Where(ml => ml.Mateid == mateid && ml.LotStatus == "activo")
            .OrderBy(ml => ml.ExpirationDate ?? DateOnly.MaxValue)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MaterialLot>> GetFifoLotsAsync(int mateid, CancellationToken ct = default) =>
        await _context.MaterialLots
            .Where(ml => ml.Mateid == mateid && ml.LotStatus == "activo" && ml.CurrentQuantity > 0)
            .OrderBy(ml => ml.ExpirationDate == null ? 1 : 0)
            .ThenBy(ml => ml.ExpirationDate)
            .ThenBy(ml => ml.EntryDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MaterialLot>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default)
    {
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));
        return await _context.MaterialLots.AsNoTracking()
            .Where(ml => ml.LotStatus == "activo" && ml.ExpirationDate.HasValue
                && ml.ExpirationDate.Value <= limitDate)
            .Include(ml => ml.Material).ThenInclude(m => m!.Category)
            .OrderBy(ml => ml.ExpirationDate)
            .ToListAsync(ct);
    }

    public async Task AddLotAsync(MaterialLot lot, CancellationToken ct = default) =>
        await _context.MaterialLots.AddAsync(lot, ct);

    public void UpdateLot(MaterialLot lot) => _context.MaterialLots.Update(lot);

    public async Task AddConsumptionAsync(MaterialConsumption consumption, CancellationToken ct = default) =>
        await _context.MaterialConsumptions.AddAsync(consumption, ct);

    public async Task AddDiscardAsync(MaterialDiscard discard, CancellationToken ct = default) =>
        await _context.MaterialDiscards.AddAsync(discard, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

// ─────────────────────────────────────────────────────────────────────────────
// AlertRepository — sin cambios respecto a versión anterior
// ─────────────────────────────────────────────────────────────────────────────

}
