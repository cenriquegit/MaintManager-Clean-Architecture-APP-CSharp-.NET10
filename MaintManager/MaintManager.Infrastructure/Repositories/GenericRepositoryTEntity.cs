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

internal sealed class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly FleetMaintenanceContext _context;
    private readonly DbSet<TEntity> _set;

    public GenericRepository(FleetMaintenanceContext context)
    {
        _context = context;
        _set = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FindAsync([id], ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await _set.AddAsync(entity, ct);

    public void Update(TEntity entity) => _set.Update(entity);
    public void Delete(TEntity entity) => _set.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

// ─────────────────────────────────────────────────────────────────────────────
// MaintenanceRepository — sin cambios respecto a versión anterior
// ─────────────────────────────────────────────────────────────────────────────

}
