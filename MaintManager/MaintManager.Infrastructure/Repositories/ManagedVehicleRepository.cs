using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;

public sealed class ManagedVehicleRepository : IManagedVehicleRepository
{
    private readonly FleetMaintenanceContext _context;

    public ManagedVehicleRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<IReadOnlyList<ManagedVehicle>> GetAllAsync(string? search, string? source, CancellationToken ct = default)
    {
        var query = _context.ManagedVehicles.AsNoTracking().Where(v => v.Status);

        if (!string.IsNullOrWhiteSpace(source) && source != "all")
            query = query.Where(v => v.Source == source);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(v => v.LicensePlate.Contains(search) || v.VehicleName.Contains(search));

        return await query.OrderBy(v => v.Source).ThenBy(v => v.CreatedAt).ToListAsync(ct);
    }

    public async Task<ManagedVehicle?> GetByIdAsync(int mvId, CancellationToken ct = default) =>
        await _context.ManagedVehicles.AsNoTracking().FirstOrDefaultAsync(v => v.MvId == mvId, ct);

    public async Task<ManagedVehicle?> GetByPrcoidAsync(int prcoid, CancellationToken ct = default) =>
        await _context.ManagedVehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Prcoid == prcoid, ct);

    public async Task<ManagedVehicle?> GetByPlateAsync(string plate, CancellationToken ct = default) =>
        await _context.ManagedVehicles.AsNoTracking().FirstOrDefaultAsync(v => v.LicensePlate == plate, ct);

    public async Task AddAsync(ManagedVehicle vehicle, CancellationToken ct = default) =>
        await _context.ManagedVehicles.AddAsync(vehicle, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
