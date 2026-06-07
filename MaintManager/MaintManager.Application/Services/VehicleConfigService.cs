using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;

namespace MaintManager.Application.Services;

public sealed class VehicleConfigService : IVehicleConfigService
{
    private readonly IVehicleConfigRepository _configRepo;

    public VehicleConfigService(IVehicleConfigRepository configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task AddActionAsync(int prcoid, int acatid, CancellationToken ct = default)
    {
        await _configRepo.AddAllowedActionAsync(prcoid, acatid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task RemoveActionAsync(int prcoid, int acatid, CancellationToken ct = default)
    {
        await _configRepo.RemoveAllowedActionAsync(prcoid, acatid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task AddMaterialAsync(int prcoid, int mateid, CancellationToken ct = default)
    {
        await _configRepo.AddAllowedMaterialAsync(prcoid, mateid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task RemoveMaterialAsync(int prcoid, int mateid, CancellationToken ct = default)
    {
        await _configRepo.RemoveAllowedMaterialAsync(prcoid, mateid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task AddComponentAsync(int prcoid, int acatid, CancellationToken ct = default)
    {
        await _configRepo.AddAllowedComponentAsync(prcoid, acatid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task RemoveComponentAsync(int prcoid, int acatid, CancellationToken ct = default)
    {
        await _configRepo.RemoveAllowedComponentAsync(prcoid, acatid, null, ct);
        await _configRepo.SaveChangesAsync(ct);
    }

    public async Task<ActionCatalog> CreateActionCatalogAsync(string name, string category,
        string? description, int? usefulLifeDays, CancellationToken ct = default)
    {
        var entity = await _configRepo.CreateActionCatalogAsync(name, category, description, usefulLifeDays, ct);
        await _configRepo.SaveChangesAsync(ct);
        return entity;
    }
}
