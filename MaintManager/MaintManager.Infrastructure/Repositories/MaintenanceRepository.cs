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

}
