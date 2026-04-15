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

}
