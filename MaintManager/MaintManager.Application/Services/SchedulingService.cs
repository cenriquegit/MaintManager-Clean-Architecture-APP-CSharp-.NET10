using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.Application.Services;

public sealed class SchedulingService : ISchedulingService
{
    private readonly FleetMaintenanceContext _context;
    private readonly IVehicleRepository _vehicleRepo;

    public SchedulingService(FleetMaintenanceContext context, IVehicleRepository vehicleRepo)
    {
        _context = context;
        _vehicleRepo = vehicleRepo;
    }

    public async Task<VehicleSchedule?> GetScheduleAsync(int prcoid, CancellationToken ct = default) =>
        await _context.VehicleSchedules
            .FirstOrDefaultAsync(vs => vs.Prcoid == prcoid && vs.Status, ct);

    public async Task<VehicleSchedule> CreateScheduleAsync(
        int prcoid, int currentKm, int createdBy, CancellationToken ct = default)
    {
        var existing = await GetScheduleAsync(prcoid, ct);
        if (existing is not null) return existing;

        var intervalKm = await GetIntervalKmFromConfigAsync(ct);
        var schedule = VehicleSchedule.Create(prcoid, currentKm, createdBy, intervalKm);
        await _context.VehicleSchedules.AddAsync(schedule, ct);
        await _context.SaveChangesAsync(ct);
        return schedule;
    }

    public async Task RescheduleAsync(int prcoid, int serviceKm, CancellationToken ct = default)
    {
        var schedule = await GetScheduleAsync(prcoid, ct);
        if (schedule is null) return;

        schedule.Reschedule(serviceKm);
        _context.VehicleSchedules.Update(schedule);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsMaintenanceDueSoonAsync(int prcoid, CancellationToken ct = default)
    {
        var schedule = await GetScheduleAsync(prcoid, ct);
        if (schedule is null) return false;

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(prcoid, ct);
        var threshold = schedule.AlertKmThreshold ?? await GetAlertThresholdFromConfigAsync(ct);

        return currentKm >= schedule.NextKm - threshold;
    }

    private async Task<int> GetIntervalKmFromConfigAsync(CancellationToken ct)
    {
        var config = await _context.ConfigSystems
            .FirstOrDefaultAsync(c => c.Key == "intervalo_km" && c.Status, ct);
        return config is not null ? config.GetIntValue() : 5000;
    }

    private async Task<int> GetAlertThresholdFromConfigAsync(CancellationToken ct)
    {
        var config = await _context.ConfigSystems
            .FirstOrDefaultAsync(c => c.Key == "alerta_km_umbral" && c.Status, ct);
        return config is not null ? config.GetIntValue() : 800;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Application/Services/InventoryService.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;


/// <summary>Gestión de inventario de materiales con FIFO por vencimiento.</summary>
