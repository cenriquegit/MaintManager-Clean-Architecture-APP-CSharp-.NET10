using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Maintenance;
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

public interface IMaintenanceRepository : IGenericRepository<Maintenance>
{
    Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<Maintenance?> GetWithDetailsAsync(int mainid, CancellationToken ct = default);
    Task<Maintenance?> GetLastByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<Maintenance>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<MaintenanceListItemDto>> GetPagedListItemsAsync(int page, int pageSize, CancellationToken ct = default);
}