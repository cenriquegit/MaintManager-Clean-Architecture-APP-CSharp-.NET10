using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Services;

public interface IVehicleConfigService
{
    Task AddActionAsync(int prcoid, int acatid, CancellationToken ct = default);
    Task RemoveActionAsync(int prcoid, int acatid, CancellationToken ct = default);
    Task AddMaterialAsync(int prcoid, int mateid, CancellationToken ct = default);
    Task RemoveMaterialAsync(int prcoid, int mateid, CancellationToken ct = default);
    Task AddComponentAsync(int prcoid, int acatid, CancellationToken ct = default);
    Task RemoveComponentAsync(int prcoid, int acatid, CancellationToken ct = default);

    Task<ActionCatalog> CreateActionCatalogAsync(string name, string category,
        string? description, int? usefulLifeDays, CancellationToken ct = default);
}
