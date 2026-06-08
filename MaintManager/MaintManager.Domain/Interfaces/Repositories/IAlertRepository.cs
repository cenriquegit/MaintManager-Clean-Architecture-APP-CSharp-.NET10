using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio del sistema de alertas.</summary>
public interface IAlertRepository
{
    Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertLog>> GetResolvedAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertLog>> GetReadAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertLog>> GetUnreadAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertLog>> GetAllAlertsAsync(CancellationToken ct = default);
    Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default);
    Task AddAsync(AlertLog alert, CancellationToken ct = default);
    void Update(AlertLog alert);

    /// <summary>Obtiene una configuración de alerta por su tipo (ej. "mantenimiento_proximo_km").</summary>
    Task<AlertConfig?> GetConfigByTypeAsync(string alertType, CancellationToken ct = default);

    /// <summary>
    /// Verifica si ya existe una alerta no resuelta para la misma configuración
    /// y la misma referencia (vehículo, material, lote, componente).
    /// </summary>
    Task<bool> ExistsUnresolvedForReferenceAsync(
        int alcoid,
        int? prcoid = null,
        int? mateid = null,
        int? maloid = null,
        int? incoid = null,
        CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}