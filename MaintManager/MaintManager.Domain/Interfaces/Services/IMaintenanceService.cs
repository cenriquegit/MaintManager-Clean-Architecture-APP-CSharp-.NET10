
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de gestión de órdenes de mantenimiento.</summary>
public interface IMaintenanceService
{
    Task<Maintenance> CreateAsync(int prcoid, short matyid, int mileage,
        int assignedTo, int registeredBy, short? setyid, string? note, CancellationToken ct = default);

    Task<Maintenance> GetWithDetailsAsync(int mainid, CancellationToken ct = default);
    Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task CompleteActionAsync(int madeid, char actionCode, string? productUsed,
        string? quantity, string? origin, string? observation, int? maloid, CancellationToken ct = default);
    Task<int> CreateActionAsync(int mainid, int actionCatalogId, CancellationToken ct = default);
    Task SaveDiagnosisAsync(int mainid, string generalStatus, bool vehicleOperative,
        string? observations, string? futureRecommendations, CancellationToken ct = default);
    Task CloseMaintenanceAsync(int mainid, bool? isEmergencyComplete, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

