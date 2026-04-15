using MaintManager.Domain.Entities.Existing;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio de vehículos (tablas existentes — solo lectura).</summary>
public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetActiveVehiclesAsync(CancellationToken ct = default);
    Task<Vehicle?> GetByIdAsync(int prcoid, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el km actual del vehículo: último kilometer_end de rentexecute
    /// o mileage del registro si no hay rentas.
    /// </summary>
    Task<int> GetCurrentKmAsync(int prcoid, CancellationToken ct = default);
}