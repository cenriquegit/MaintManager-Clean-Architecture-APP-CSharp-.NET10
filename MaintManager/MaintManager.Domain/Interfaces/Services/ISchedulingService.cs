
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de calendarización y recalendarización.</summary>
public interface ISchedulingService
{
    Task<VehicleSchedule?> GetScheduleAsync(int prcoid, CancellationToken ct = default);
    Task<VehicleSchedule> CreateScheduleAsync(int prcoid, int currentKm, int createdBy, CancellationToken ct = default);
    Task RescheduleAsync(int prcoid, int serviceKm, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un vehículo está próximo a su próximo mantenimiento
    /// según el umbral configurado en config_system.
    /// </summary>
    Task<bool> IsMaintenanceDueSoonAsync(int prcoid, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

