using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;

public sealed class VehicleConfigRepository : IVehicleConfigRepository
{
    private readonly FleetMaintenanceContext _context;

    public VehicleConfigRepository(FleetMaintenanceContext context) => _context = context;

    // ── Actions ────────────────────────────────────────────────

    public async Task<IReadOnlyList<VehicleAllowedAction>> GetAllowedActionsAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedAction>()
            .AsNoTracking()
            .Where(v => v.Prcoid == prcoid)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VehicleAllowedAction>> GetAllowedActionsByMvIdAsync(int mvId, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedAction>()
            .AsNoTracking()
            .Where(v => v.MvId == mvId)
            .ToListAsync(ct);

    public async Task AddAllowedActionAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedAction>().AddAsync(
            VehicleAllowedAction.Create(prcoid, acatid, mvId), ct);

    public async Task RemoveAllowedActionAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default)
    {
        var entity = await _context.Set<VehicleAllowedAction>()
            .FirstOrDefaultAsync(v =>
                (prcoid.HasValue && v.Prcoid == prcoid.Value && v.Acatid == acatid) ||
                (mvId.HasValue && v.MvId == mvId.Value && v.Acatid == acatid), ct);
        if (entity is not null)
            _context.Set<VehicleAllowedAction>().Remove(entity);
    }

    // ── Materials ──────────────────────────────────────────────

    public async Task<IReadOnlyList<VehicleAllowedMaterial>> GetAllowedMaterialsAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedMaterial>()
            .AsNoTracking()
            .Where(v => v.Prcoid == prcoid)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VehicleAllowedMaterial>> GetAllowedMaterialsByMvIdAsync(int mvId, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedMaterial>()
            .AsNoTracking()
            .Where(v => v.MvId == mvId)
            .ToListAsync(ct);

    public async Task AddAllowedMaterialAsync(int? prcoid, int mateid, int? mvId = null, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedMaterial>().AddAsync(
            VehicleAllowedMaterial.Create(prcoid, mateid, mvId), ct);

    public async Task RemoveAllowedMaterialAsync(int? prcoid, int mateid, int? mvId = null, CancellationToken ct = default)
    {
        var entity = await _context.Set<VehicleAllowedMaterial>()
            .FirstOrDefaultAsync(v =>
                (prcoid.HasValue && v.Prcoid == prcoid.Value && v.Mateid == mateid) ||
                (mvId.HasValue && v.MvId == mvId.Value && v.Mateid == mateid), ct);
        if (entity is not null)
            _context.Set<VehicleAllowedMaterial>().Remove(entity);
    }

    // ── Components ─────────────────────────────────────────────

    public async Task<IReadOnlyList<VehicleAllowedComponent>> GetAllowedComponentsAsync(int prcoid, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedComponent>()
            .AsNoTracking()
            .Where(v => v.Prcoid == prcoid)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VehicleAllowedComponent>> GetAllowedComponentsByMvIdAsync(int mvId, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedComponent>()
            .AsNoTracking()
            .Where(v => v.MvId == mvId)
            .ToListAsync(ct);

    public async Task AddAllowedComponentAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default) =>
        await _context.Set<VehicleAllowedComponent>().AddAsync(
            VehicleAllowedComponent.Create(prcoid, acatid, mvId), ct);

    public async Task RemoveAllowedComponentAsync(int? prcoid, int acatid, int? mvId = null, CancellationToken ct = default)
    {
        var entity = await _context.Set<VehicleAllowedComponent>()
            .FirstOrDefaultAsync(v =>
                (prcoid.HasValue && v.Prcoid == prcoid.Value && v.Acatid == acatid) ||
                (mvId.HasValue && v.MvId == mvId.Value && v.Acatid == acatid), ct);
        if (entity is not null)
            _context.Set<VehicleAllowedComponent>().Remove(entity);
    }

    // ── Action Catalog ──────────────────────────────────────────

    public async Task<ActionCatalog> CreateActionCatalogAsync(string name, string category,
        string? description, int? usefulLifeDays, CancellationToken ct = default)
    {
        var entity = ActionCatalog.Create(name, category, description, usefulLifeDays);
        _context.ActionCatalogs.Add(entity);
        return entity;
    }

    public async Task AddActionCatalogAsync(ActionCatalog catalog, CancellationToken ct = default) =>
        await _context.ActionCatalogs.AddAsync(catalog, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
