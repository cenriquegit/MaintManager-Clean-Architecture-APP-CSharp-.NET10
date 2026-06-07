using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

public interface IManagedVehicleRepository
{
    Task<IReadOnlyList<ManagedVehicle>> GetAllAsync(string? search, string? source, CancellationToken ct = default);
    Task<ManagedVehicle?> GetByIdAsync(int mvId, CancellationToken ct = default);
    Task<ManagedVehicle?> GetByPrcoidAsync(int prcoid, CancellationToken ct = default);
    Task<ManagedVehicle?> GetByPlateAsync(string plate, CancellationToken ct = default);
    Task AddAsync(ManagedVehicle vehicle, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
