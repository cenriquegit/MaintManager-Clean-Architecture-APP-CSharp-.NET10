using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly FleetMaintenanceContext _context;

    public AlertRepository(FleetMaintenanceContext context) => _context = context;

    public async Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default) =>
        await _context.AlertLogs.AsNoTracking()
            .Where(al => !al.Resolved)
            .Include(al => al.AlertConfig)
            .OrderByDescending(al => al.AlertDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AlertLog>> GetResolvedAlertsAsync(CancellationToken ct = default) =>
        await _context.AlertLogs.AsNoTracking()
            .Where(al => al.Resolved)
            .Include(al => al.AlertConfig)
            .OrderByDescending(al => al.ResolvedAt)
            .ToListAsync(ct);

    public async Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default) =>
        await _context.AlertLogs.Include(al => al.AlertConfig)
            .FirstOrDefaultAsync(al => al.Alloid == alloid, ct);

    public async Task AddAsync(AlertLog alert, CancellationToken ct = default) =>
        await _context.AlertLogs.AddAsync(alert, ct);

    public void Update(AlertLog alert) => _context.AlertLogs.Update(alert);

    public async Task<AlertConfig?> GetConfigByTypeAsync(string alertType, CancellationToken ct = default) =>
        await _context.AlertConfigs
            .FirstOrDefaultAsync(ac => ac.AlertType == alertType && ac.Enabled, ct);

    public async Task<bool> ExistsUnresolvedForReferenceAsync(
        int alcoid,
        int? prcoid = null,
        int? mateid = null,
        int? maloid = null,
        int? incoid = null,
        CancellationToken ct = default)
    {
        return await _context.AlertLogs.AnyAsync(al =>
            al.Alcoid == alcoid &&
            (prcoid == null || al.Prcoid == prcoid) &&
            (mateid == null || al.Mateid == mateid) &&
            (maloid == null || al.Maloid == maloid) &&
            (incoid == null || al.Incoid == incoid) &&
            !al.Resolved, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}