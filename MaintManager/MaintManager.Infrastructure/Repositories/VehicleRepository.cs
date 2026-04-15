using MaintManager.Domain.Entities.Existing;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;

public sealed class VehicleRepository : IVehicleRepository
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
        var vehicle = await _context.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Prcoid == prcoid, ct);

        if (vehicle is null) return 0;

        var lastKm = await _context.RentExecutes.AsNoTracking()
            .Where(re =>
                re.RentRequest != null &&
                re.RentRequest.Prodid == vehicle.Prodid &&
                re.KilometerEnd.HasValue &&
                re.Statid != "CA")
            .OrderByDescending(re => re.ReturnDate)
            .Select(re => re.KilometerEnd)
            .FirstOrDefaultAsync(ct);

        return lastKm ?? vehicle.Mileage ?? 0;
    }
}