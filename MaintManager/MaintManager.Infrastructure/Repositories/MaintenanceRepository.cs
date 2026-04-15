using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;

public sealed class MaintenanceRepository : GenericRepository<Maintenance>, IMaintenanceRepository
{
    public MaintenanceRepository(FleetMaintenanceContext context) : base(context)
    {
    }

    // Métodos específicos de IMaintenanceRepository (conservando TODA tu lógica original)

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
            .Include(m => m.MaterialConsumptions)          // ← conservado
            .Include(m => m.InstalledComponents).ThenInclude(ic => ic.ActionCatalog)  // ← conservado
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
}