
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio de órdenes de mantenimiento con consultas especializadas.</summary>
public interface IMaintenanceRepository : IGenericRepository<Maintenance>
{
    Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<Maintenance?> GetWithDetailsAsync(int mainid, CancellationToken ct = default);
    Task<Maintenance?> GetLastByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<Maintenance>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

