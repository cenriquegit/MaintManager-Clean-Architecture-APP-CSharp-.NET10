using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

public interface IVehicleConfigRepository
{
    Task<IReadOnlyList<VehicleAllowedAction>> GetAllowedActionsAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleAllowedAction>> GetAllowedActionsByMvIdAsync(int mvId, CancellationToken ct = default);
    Task AddAllowedActionAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default);
    Task RemoveAllowedActionAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default);

    Task<IReadOnlyList<VehicleAllowedMaterial>> GetAllowedMaterialsAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleAllowedMaterial>> GetAllowedMaterialsByMvIdAsync(int mvId, CancellationToken ct = default);
    Task AddAllowedMaterialAsync(int? prcoid, int mateid, int? mvId = null, CancellationToken ct = default);
    Task RemoveAllowedMaterialAsync(int? prcoid, int mateid, int? mvId = null, CancellationToken ct = default);

    Task<IReadOnlyList<VehicleAllowedComponent>> GetAllowedComponentsAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleAllowedComponent>> GetAllowedComponentsByMvIdAsync(int mvId, CancellationToken ct = default);
    Task AddAllowedComponentAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default);
    Task RemoveAllowedComponentAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default);

    Task<ActionCatalog> CreateActionCatalogAsync(string name, string category,
        string? description, int? usefulLifeDays, CancellationToken ct = default);
    Task AddActionCatalogAsync(ActionCatalog catalog, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
