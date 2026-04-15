// MaintManager.Application/Services/MaintenanceService.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.Application.Services;

/// <summary>Gestión de órdenes de mantenimiento y sus operaciones.</summary>
public sealed class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ISchedulingService _schedulingService;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepo,
        IVehicleRepository vehicleRepo,
        ISchedulingService schedulingService)
    {
        _maintenanceRepo = maintenanceRepo;
        _vehicleRepo = vehicleRepo;
        _schedulingService = schedulingService;
    }

    public async Task<Maintenance> CreateAsync(
        int prcoid, short matyid, int mileage,
        int assignedTo, int registeredBy,
        short? setyid, string? note,
        CancellationToken ct = default)
    {
        var lastMaintenance = await _maintenanceRepo.GetLastByVehicleAsync(prcoid, ct);
        int? kmSinceLast = lastMaintenance is not null
            ? mileage - lastMaintenance.Mileage
            : null;

        var maintenance = Maintenance.Create(
            prcoid, matyid, mileage, assignedTo, registeredBy,
            setyid: setyid, kmSinceLast: kmSinceLast, note: note);

        await _maintenanceRepo.AddAsync(maintenance, ct);
        await _maintenanceRepo.SaveChangesAsync(ct);

        return maintenance;
    }

    public async Task<Maintenance> GetWithDetailsAsync(int mainid, CancellationToken ct = default)
    {
        var maintenance = await _maintenanceRepo.GetWithDetailsAsync(mainid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Maintenance.NotFound);

        return maintenance;
    }

    public async Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default) =>
        await _maintenanceRepo.GetByVehicleAsync(prcoid, ct);

    public async Task CompleteActionAsync(
        int madeid, char actionCode, string? productUsed,
        string? quantity, string? origin, string? observation, int? maloid,
        CancellationToken ct = default)
    {
        // Buscamos el mantenimiento que contiene este detalle
        var maintenance = await FindMaintenanceByActionAsync(madeid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Maintenance.NotFound);

        var detail = maintenance.ActionDetails.FirstOrDefault(d => d.Madeid == madeid)
            ?? throw new KeyNotFoundException("Acción no encontrada en la orden.");

        detail.Complete(actionCode, productUsed, quantity, origin, observation, maloid);
        await _maintenanceRepo.SaveChangesAsync(ct);
    }

    public async Task SaveDiagnosisAsync(
        int mainid, string generalStatus, bool vehicleOperative,
        string? observations, string? futureRecommendations,
        CancellationToken ct = default)
    {
        var maintenance = await _maintenanceRepo.GetWithDetailsAsync(mainid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Maintenance.NotFound);

        if (maintenance.Diagnosis is not null)
            throw new InvalidOperationException("Esta orden ya tiene un diagnóstico registrado.");

        var diagnosis = Diagnosis.Create(mainid, generalStatus, vehicleOperative,
            observations, futureRecommendations);

        // EF Core trackea el navigation property
        maintenance.ActionDetails.GetType(); // fuerza inicialización
        await _maintenanceRepo.SaveChangesAsync(ct);
    }

    public async Task CloseMaintenanceAsync(
        int mainid, bool? isEmergencyComplete, CancellationToken ct = default)
    {
        var maintenance = await _maintenanceRepo.GetWithDetailsAsync(mainid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Maintenance.NotFound);

        if (maintenance.Diagnosis is null)
            throw new InvalidOperationException(ErrorMessages.Maintenance.DiagnosisRequired);

        if (maintenance.Matyid == 2)
        {
            if (!isEmergencyComplete.HasValue)
                throw new InvalidOperationException("Debe indicar si la emergencia fue completa o parcial.");

            maintenance.CloseEmergency(isEmergencyComplete.Value);

            // Recalendarizar solo si la emergencia fue completa
            if (isEmergencyComplete.Value)
                await _schedulingService.RescheduleAsync(maintenance.Prcoid, maintenance.Mileage, ct);
        }
        else
        {
            // Calendarizado: siempre recalendariza
            await _schedulingService.RescheduleAsync(maintenance.Prcoid, maintenance.Mileage, ct);
        }

        await _maintenanceRepo.SaveChangesAsync(ct);
    }

    private async Task<Maintenance?> FindMaintenanceByActionAsync(int madeid, CancellationToken ct)
    {
        // Buscamos a través del detalle (simplificado — en producción se optimizaría con una query directa)
        var allMaintenances = await _maintenanceRepo.GetAllAsync(ct);
        foreach (var m in allMaintenances)
        {
            var details = await _maintenanceRepo.GetWithDetailsAsync(m.Mainid, ct);
            if (details?.ActionDetails.Any(d => d.Madeid == madeid) == true)
                return details;
        }
        return null;
    }
}

