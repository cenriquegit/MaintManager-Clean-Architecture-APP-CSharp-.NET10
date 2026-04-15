
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio del sistema de alertas.</summary>
public interface IAlertRepository
{
    Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default);
    Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default);
    Task AddAsync(AlertLog alert, CancellationToken ct = default);
    void Update(AlertLog alert);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

