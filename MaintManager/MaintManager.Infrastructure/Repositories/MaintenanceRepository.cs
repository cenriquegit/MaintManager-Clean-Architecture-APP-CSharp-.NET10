using System.Linq;
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Maintenance;
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

    public async Task<PagedResult<MaintenanceListItemDto>> GetPagedListItemsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = from m in _context.Maintenances
                    join v in _context.Vehicles on m.Prcoid equals v.Prcoid
                    join p in _context.Products on v.Prodid equals p.Prodid
                    join mt in _context.MaintenanceTypes on m.Matyid equals mt.Matyid
                    join st in _context.ServiceTypes on m.Setyid equals st.Setyid into stg
                    from st in stg.DefaultIfEmpty()
                    join w in _context.Workers on m.AssignedTo equals w.Workid into wg
                    from w in wg.DefaultIfEmpty()
                    join per in _context.Persons on w.Persid equals per.Persid into perg
                    from per in perg.DefaultIfEmpty()
                    where m.Statid == "AC"
                    orderby m.MaintenanceDate descending
                    select new MaintenanceListItemDto
                    {
                        Mainid = m.Mainid,
                        LicensePlate = v.LicensePlateNumber,
                        VehicleName = p.Name,
                        MaintenanceType = mt.Name,
                        ServiceType = st.Name,
                        MaintenanceDate = m.MaintenanceDate,
                        Mileage = m.Mileage,
                        AssignedToName = per != null ? per.Name : "Sin asignar",
                        Status = m.Statid
                    };

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

        return new PagedResult<MaintenanceListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}