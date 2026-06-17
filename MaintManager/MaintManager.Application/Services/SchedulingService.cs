using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;

namespace MaintManager.Application.Services;

public sealed class SchedulingService : ISchedulingService
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IGenericRepository<VehicleSchedule> _scheduleRepo;
    private readonly IGenericRepository<ConfigSystem> _configRepo;
    private readonly IGenericRepository<Maintenance> _maintenanceRepo;

    public SchedulingService(
        IVehicleRepository vehicleRepo,
        IGenericRepository<VehicleSchedule> scheduleRepo,
        IGenericRepository<ConfigSystem> configRepo,
        IGenericRepository<Maintenance> maintenanceRepo)
    {
        _vehicleRepo = vehicleRepo;
        _scheduleRepo = scheduleRepo;
        _configRepo = configRepo;
        _maintenanceRepo = maintenanceRepo;
    }

    public async Task<VehicleSchedule?> GetScheduleAsync(int prcoid, CancellationToken ct = default)
    {
        var schedules = await _scheduleRepo.FindAsync(vs => vs.Prcoid == prcoid && vs.Status, ct);
        return schedules.FirstOrDefault();
    }

    public async Task<VehicleSchedule> CreateScheduleAsync(
        int prcoid, int currentKm, int createdBy, CancellationToken ct = default)
    {
        var existing = await GetScheduleAsync(prcoid, ct);
        if (existing is not null) return existing;

        var intervalKm = await GetIntervalKmFromConfigAsync(ct);
        var initialServiceType = await DetermineInitialServiceTypeAsync(prcoid, ct);
        var schedule = VehicleSchedule.Create(prcoid, currentKm, createdBy, intervalKm,
            initialServiceType: initialServiceType);
        await _scheduleRepo.AddAsync(schedule, ct);
        await _scheduleRepo.SaveChangesAsync(ct);
        return schedule;
    }

    public async Task RescheduleAsync(int prcoid, int serviceKm, CancellationToken ct = default)
    {
        var schedule = await GetScheduleAsync(prcoid, ct);
        if (schedule is null) return;

        schedule.Reschedule(serviceKm); // Ahora también alterna NextServiceTypeCode
        _scheduleRepo.Update(schedule);
        await _scheduleRepo.SaveChangesAsync(ct);
    }

    public async Task<bool> IsMaintenanceDueSoonAsync(int prcoid, CancellationToken ct = default)
    {
        var schedule = await GetScheduleAsync(prcoid, ct);
        if (schedule is null) return false;

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(prcoid, ct);
        var threshold = schedule.AlertKmThreshold ?? await GetAlertThresholdFromConfigAsync(ct);

        return currentKm >= schedule.NextKm - threshold;
    }

    private async Task<string> DetermineInitialServiceTypeAsync(int prcoid, CancellationToken ct)
    {
        var lastCompleted = await _maintenanceRepo
            .FindAsync(m => m.Prcoid == prcoid && m.Statid == "FI" && m.Setyid.HasValue, ct);

        var last = lastCompleted
            .OrderByDescending(m => m.MaintenanceDate)
            .FirstOrDefault();

        if (last?.Setyid == 1) return "B";
        if (last?.Setyid == 2) return "A";
        return "A";
    }

    private async Task<int> GetIntervalKmFromConfigAsync(CancellationToken ct)
    {
        var configs = await _configRepo.FindAsync(c => c.Key == "intervalo_km" && c.Status, ct);
        var config = configs.FirstOrDefault();
        return config?.GetIntValue() ?? 5000;
    }

    private async Task<int> GetAlertThresholdFromConfigAsync(CancellationToken ct)
    {
        var configs = await _configRepo.FindAsync(c => c.Key == "alerta_km_umbral" && c.Status, ct);
        var config = configs.FirstOrDefault();
        return config?.GetIntValue() ?? 800;
    }
}