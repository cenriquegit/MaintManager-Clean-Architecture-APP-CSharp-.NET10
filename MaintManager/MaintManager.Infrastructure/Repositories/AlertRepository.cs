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

internal sealed class AlertRepository : IAlertRepository
{
    private readonly FleetMaintenanceContext _context;

    public AlertRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default) =>
        await _context.AlertLogs.AsNoTracking()
            .Where(al => !al.Resolved)
            .Include(al => al.AlertConfig)
            .OrderByDescending(al => al.AlertDate)
            .ToListAsync(ct);

    public async Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default) =>
        await _context.AlertLogs.Include(al => al.AlertConfig)
            .FirstOrDefaultAsync(al => al.Alloid == alloid, ct);

    public async Task AddAsync(AlertLog alert, CancellationToken ct = default) =>
        await _context.AlertLogs.AddAsync(alert, ct);

    public void Update(AlertLog alert) => _context.AlertLogs.Update(alert);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
}
