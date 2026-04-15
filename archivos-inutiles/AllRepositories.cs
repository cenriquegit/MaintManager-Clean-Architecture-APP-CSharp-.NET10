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

// ─────────────────────────────────────────────────────────────────────────────
// GenericRepository
// ─────────────────────────────────────────────────────────────────────────────

internal sealed class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly FleetMaintenanceContext _context;
    private readonly DbSet<TEntity> _set;

    public GenericRepository(FleetMaintenanceContext context)
    {
        _context = context;
        _set = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await _set.AddAsync(entity, ct);

    public void Update(TEntity entity) => _set.Update(entity);
    public void Delete(TEntity entity) => _set.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

// ─────────────────────────────────────────────────────────────────────────────
// MaintenanceRepository — sin cambios respecto a versión anterior
// ─────────────────────────────────────────────────────────────────────────────

internal sealed class MaintenanceRepository : IMaintenanceRepository
{
    private readonly FleetMaintenanceContext _context;

    public MaintenanceRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<Maintenance?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Maintenances.FindAsync([id], ct);

    public async Task<IReadOnlyList<Maintenance>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Maintenances.AsNoTracking()
            .OrderByDescending(m => m.MaintenanceDate).ToListAsync(ct);

    public async Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Maintenances.AsNoTracking()
            .Where(m => m.Prcoid == prcoid && m.Statid == "AC")
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .OrderByDescending(m => m.MaintenanceDate)
            .ToListAsync(ct);

    public async Task<Maintenance?> GetWithDetailsAsync(int mainid, CancellationToken ct = default) =>
        await _context.Maintenances
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .Include(m => m.ActionDetails).ThenInclude(d => d.ActionCatalog)
            .Include(m => m.Diagnosis)
            .Include(m => m.MaterialConsumptions)
            .Include(m => m.InstalledComponents).ThenInclude(ic => ic.ActionCatalog)
            .FirstOrDefaultAsync(m => m.Mainid == mainid, ct);

    public async Task<Maintenance?> GetLastByVehicleAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Maintenances.AsNoTracking()
            .Where(m => m.Prcoid == prcoid && m.Statid == "AC")
            .OrderByDescending(m => m.MaintenanceDate)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Maintenance>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) =>
        await _context.Maintenances.AsNoTracking()
            .Where(m => m.Statid == "AC")
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .OrderByDescending(m => m.MaintenanceDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

    public async Task AddAsync(Maintenance entity, CancellationToken ct = default) =>
        await _context.Maintenances.AddAsync(entity, ct);

    public void Update(Maintenance entity) => _context.Maintenances.Update(entity);
    public void Delete(Maintenance entity) => _context.Maintenances.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

// ─────────────────────────────────────────────────────────────────────────────
// VehicleRepository — ACTUALIZADO para BD-FINAL
// Cambios:
// 1. GetCurrentKmAsync: join correcto rentexecute→rentrequest→prodid
//    (la nueva BD tiene FK explícita rentrequest→prodid para obtener el vehículo)
// 2. GetActiveVehiclesAsync: incluye Product para obtener el nombre del vehículo
// ─────────────────────────────────────────────────────────────────────────────

internal sealed class VehicleRepository : IVehicleRepository
{
    private readonly FleetMaintenanceContext _context;

    public VehicleRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<IReadOnlyList<Vehicle>> GetActiveVehiclesAsync(CancellationToken ct = default) =>
        await _context.Vehicles.AsNoTracking()
            .Where(v => v.Status)
            .Include(v => v.Product)
            .OrderBy(v => v.LicensePlateNumber)
            .ToListAsync(ct);

    public async Task<Vehicle?> GetByIdAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Vehicles.AsNoTracking()
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Prcoid == prcoid, ct);

    public async Task<int> GetCurrentKmAsync(int prcoid, CancellationToken ct = default)
    {
        // Obtener el prodid del vehículo (vehicle hereda de company, prodid está disponible)
        var vehicle = await _context.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Prcoid == prcoid, ct);

        if (vehicle is null) return 0;

        // Último kilometer_end de una renta completada (no cancelada) para este vehículo.
        // BD-FINAL: rentexecute.sereid → rentrequest.sereid → rentrequest.prodid = vehicle.prodid
        var lastKm = await _context.RentExecutes.AsNoTracking()
            .Where(re =>
                re.RentRequest != null &&
                re.RentRequest.Prodid == vehicle.Prodid &&
                re.KilometerEnd.HasValue &&
                re.Statid != "CA")
            .OrderByDescending(re => re.ReturnDate)
            .Select(re => re.KilometerEnd)
            .FirstOrDefaultAsync(ct);

        // Si no hay rentas, usar el mileage registrado del vehículo
        return lastKm ?? vehicle.Mileage ?? 0;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// InventoryRepository — sin cambios respecto a versión anterior
// ─────────────────────────────────────────────────────────────────────────────

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

internal sealed class AlertRepository : IAlertRepository
{
    private readonly FleetMaintenanceContext _context;

    public AlertRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default) =>
        await _context.AlertLogs.AsNoTracking()
            .Where(al => !al.Resolved)
            .Include(al => al.AlertConfig)
            .OrderByDescending(al => al.AlertDate)
            .ToListAsync(ct);

    public async Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default) =>
        await _context.AlertLogs.Include(al => al.AlertConfig)
            .FirstOrDefaultAsync(al => al.Alloid == alloid, ct);

    public async Task AddAsync(AlertLog alert, CancellationToken ct = default) =>
        await _context.AlertLogs.AddAsync(alert, ct);

    public void Update(AlertLog alert) => _context.AlertLogs.Update(alert);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}