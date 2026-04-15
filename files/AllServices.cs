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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Application/Services/SchedulingService.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Application.Services;

/// <summary>Calendarización y recalendarización automática de mantenimientos.</summary>
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

namespace MaintManager.Application.Services;

/// <summary>Gestión de inventario de materiales con FIFO por vencimiento.</summary>
public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;

    public InventoryService(IInventoryRepository inventoryRepo) =>
        _inventoryRepo = inventoryRepo;

    public async Task CreateMaterialAsync(
        short macaid, string name, string unit, decimal stockMin,
        int createdBy, CancellationToken ct = default)
    {
        var material = Material.Create(macaid, name, unit, stockMin, createdBy);
        await _inventoryRepo.AddMaterialAsync(material, ct);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task RegisterLotAsync(
        int mateid, decimal quantity, decimal unitCost, int createdBy,
        DateOnly? expirationDate, int? provid, string? supplierLot,
        CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        var lot = MaterialLot.Create(mateid, quantity, unitCost, createdBy,
            expirationDate, provid, supplierLot);

        await _inventoryRepo.AddLotAsync(lot, ct);
        material.AddStock(quantity);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<int>> ConsumeStockFifoAsync(
        int mateid, decimal quantity, int mainid, CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        if (material.StockTotal < quantity)
            throw new InvalidOperationException(ErrorMessages.Inventory.InsufficientStock);

        var lots = await _inventoryRepo.GetFifoLotsAsync(mateid, ct);
        var consumedLotIds = new List<int>();
        var remaining = quantity;

        foreach (var lot in lots)
        {
            if (remaining <= 0) break;

            var toConsume = Math.Min(remaining, lot.CurrentQuantity);
            lot.Consume(toConsume);
            _inventoryRepo.UpdateLot(lot);

            var consumption = MaterialConsumption.Create(mainid, mateid, toConsume, "Stock propio", lot.Maloid);
            await _inventoryRepo.AddConsumptionAsync(consumption, ct);
            consumedLotIds.Add(lot.Maloid);
            remaining -= toConsume;
        }

        material.DeductStock(quantity);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);

        return consumedLotIds;
    }

    public async Task DiscardLotAsync(
        int maloid, decimal quantity, string reason, int discardedBy,
        string? note, CancellationToken ct = default)
    {
        var lot = await _inventoryRepo.GetLotByIdAsync(maloid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.LotNotFound);

        if (lot.LotStatus == "vencido" || lot.LotStatus == "descartado")
            throw new InvalidOperationException(ErrorMessages.Inventory.LotExpired);

        var material = await _inventoryRepo.GetMaterialByIdAsync(lot.Mateid, ct)!;
        lot.Discard(quantity);
        material!.DeductStock(quantity);

        var discard = MaterialDiscard.Create(maloid, quantity, reason, discardedBy, note);
        await _inventoryRepo.AddDiscardAsync(discard, ct);
        _inventoryRepo.UpdateLot(lot);
        _inventoryRepo.UpdateMaterial(material);
        await _inventoryRepo.SaveChangesAsync(ct);
    }

    public async Task RateMaterialAsync(
        int mateid, int mainid, short rating, int ratedBy,
        string? observation, CancellationToken ct = default)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(mateid, ct)
            ?? throw new KeyNotFoundException(ErrorMessages.Inventory.MaterialNotFound);

        var materialRating = MaterialRating.Create(mateid, mainid, rating, ratedBy, observation);
        // Se agrega via el contexto — EF Core lo maneja con la relación
        await _inventoryRepo.SaveChangesAsync(ct);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Application/Services/AlertService.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Application.Services;

/// <summary>Generación y gestión del sistema de alertas.</summary>
public sealed class AlertService : IAlertService
{
    private readonly FleetMaintenanceContext _context;
    private readonly IAlertRepository _alertRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ISchedulingService _schedulingService;

    public AlertService(
        FleetMaintenanceContext context,
        IAlertRepository alertRepo,
        IVehicleRepository vehicleRepo,
        ISchedulingService schedulingService)
    {
        _context = context;
        _alertRepo = alertRepo;
        _vehicleRepo = vehicleRepo;
        _schedulingService = schedulingService;
    }

    /// <summary>Ejecuta todas las verificaciones y genera alertas pendientes.</summary>
    public async Task CheckAndGenerateAlertsAsync(CancellationToken ct = default)
    {
        await CheckMaintenanceDueSoonAsync(ct);
        await CheckExpiringLotsAsync(ct);
        await CheckLowStockAsync(ct);
        await CheckExpiringComponentsAsync(ct);
        await _alertRepo.SaveChangesAsync(ct);
    }

    public async Task MarkAsReadAsync(int alloid, int readByWorkid, CancellationToken ct = default)
    {
        var alert = await _alertRepo.GetByIdAsync(alloid, ct)
            ?? throw new KeyNotFoundException("Alerta no encontrada.");
        alert.MarkAsRead(readByWorkid);
        _alertRepo.Update(alert);
        await _alertRepo.SaveChangesAsync(ct);
    }

    public async Task ResolveAsync(int alloid, int resolvedByWorkid, CancellationToken ct = default)
    {
        var alert = await _alertRepo.GetByIdAsync(alloid, ct)
            ?? throw new KeyNotFoundException("Alerta no encontrada.");
        alert.Resolve(resolvedByWorkid);
        _alertRepo.Update(alert);
        await _alertRepo.SaveChangesAsync(ct);
    }

    private async Task CheckMaintenanceDueSoonAsync(CancellationToken ct)
    {
        var alertConfig = await _context.AlertConfigs
            .FirstOrDefaultAsync(ac => ac.AlertType == AlertTypes.MantenimientoProximoKm && ac.Enabled, ct);
        if (alertConfig is null) return;

        var vehicles = await _vehicleRepo.GetActiveVehiclesAsync(ct);
        foreach (var vehicle in vehicles)
        {
            if (!await _schedulingService.IsMaintenanceDueSoonAsync(vehicle.Prcoid, ct)) continue;

            var alreadyExists = await _context.AlertLogs.AnyAsync(
                al => al.Alcoid == alertConfig.Alcoid
                    && al.Prcoid == vehicle.Prcoid
                    && !al.Resolved, ct);

            if (alreadyExists) continue;

            var currentKm = await _vehicleRepo.GetCurrentKmAsync(vehicle.Prcoid, ct);
            var schedule = await _schedulingService.GetScheduleAsync(vehicle.Prcoid, ct);
            var message = $"El vehículo {vehicle.LicensePlateNumber} tiene mantenimiento próximo. " +
                          $"Km actual: {currentKm}. Próximo servicio: {schedule?.NextKm} km.";

            var alert = AlertLog.Create(alertConfig.Alcoid, message, prcoid: vehicle.Prcoid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }

    private async Task CheckExpiringLotsAsync(CancellationToken ct)
    {
        var alertConfig = await _context.AlertConfigs
            .FirstOrDefaultAsync(ac => ac.AlertType == AlertTypes.LotePorVencer && ac.Enabled, ct);
        if (alertConfig is null) return;

        var daysThreshold = int.TryParse(alertConfig.ThresholdValue, out var days) ? days : 30;
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));

        var expiringLots = await _context.MaterialLots
            .Where(ml => ml.LotStatus == "activo"
                && ml.ExpirationDate.HasValue
                && ml.ExpirationDate.Value <= limitDate)
            .Include(ml => ml.Material)
            .ToListAsync(ct);

        foreach (var lot in expiringLots)
        {
            var alreadyExists = await _context.AlertLogs.AnyAsync(
                al => al.Alcoid == alertConfig.Alcoid
                    && al.Maloid == lot.Maloid
                    && !al.Resolved, ct);
            if (alreadyExists) continue;

            var message = $"Lote de {lot.Material?.Name} vence el {lot.ExpirationDate:dd/MM/yyyy}. " +
                          $"Cantidad restante: {lot.CurrentQuantity} {lot.Material?.UnitOfMeasure}.";
            var alert = AlertLog.Create(alertConfig.Alcoid, message,
                mateid: lot.Mateid, maloid: lot.Maloid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }

    private async Task CheckLowStockAsync(CancellationToken ct)
    {
        var alertConfig = await _context.AlertConfigs
            .FirstOrDefaultAsync(ac => ac.AlertType == AlertTypes.StockBajo && ac.Enabled, ct);
        if (alertConfig is null) return;

        var lowStockMaterials = await _context.Materials
            .Where(m => m.Status && m.StockTotal < m.StockMinimum)
            .ToListAsync(ct);

        foreach (var material in lowStockMaterials)
        {
            var alreadyExists = await _context.AlertLogs.AnyAsync(
                al => al.Alcoid == alertConfig.Alcoid
                    && al.Mateid == material.Mateid
                    && !al.Resolved, ct);
            if (alreadyExists) continue;

            var message = $"Stock bajo de {material.Name}. " +
                          $"Actual: {material.StockTotal} {material.UnitOfMeasure}. " +
                          $"Mínimo: {material.StockMinimum}.";
            var alert = AlertLog.Create(alertConfig.Alcoid, message, mateid: material.Mateid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }

    private async Task CheckExpiringComponentsAsync(CancellationToken ct)
    {
        var alertConfig = await _context.AlertConfigs
            .FirstOrDefaultAsync(ac => ac.AlertType == AlertTypes.ComponentePorCaducar && ac.Enabled, ct);
        if (alertConfig is null) return;

        var daysThreshold = int.TryParse(alertConfig.ThresholdValue, out var days) ? days : 30;
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));

        var expiringComponents = await _context.InstalledComponents
            .Where(ic => ic.Active
                && ic.ExpirationDate.HasValue
                && ic.ExpirationDate.Value <= limitDate)
            .Include(ic => ic.ActionCatalog)
            .ToListAsync(ct);

        foreach (var component in expiringComponents)
        {
            var alreadyExists = await _context.AlertLogs.AnyAsync(
                al => al.Alcoid == alertConfig.Alcoid
                    && al.Incoid == component.Incoid
                    && !al.Resolved, ct);
            if (alreadyExists) continue;

            var message = $"Componente '{component.ActionCatalog?.Name}' en vehículo {component.Prcoid} " +
                          $"caduca el {component.ExpirationDate:dd/MM/yyyy}.";
            var alert = AlertLog.Create(alertConfig.Alcoid, message,
                prcoid: component.Prcoid, incoid: component.Incoid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }
}
