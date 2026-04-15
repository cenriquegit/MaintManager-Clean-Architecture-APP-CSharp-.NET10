using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.Application.Services;

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
