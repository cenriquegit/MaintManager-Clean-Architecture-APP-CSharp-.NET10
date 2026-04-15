using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.Application.Services;

/// <summary>Generación y gestión del sistema de alertas.</summary>
public sealed class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly ISchedulingService _schedulingService;

    public AlertService(
        IAlertRepository alertRepo,
        IVehicleRepository vehicleRepo,
        IInventoryRepository inventoryRepo,
        ISchedulingService schedulingService)
    {
        _alertRepo = alertRepo;
        _vehicleRepo = vehicleRepo;
        _inventoryRepo = inventoryRepo;
        _schedulingService = schedulingService;
    }

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
        var alertConfig = await _alertRepo.GetConfigByTypeAsync(AlertTypes.MantenimientoProximoKm, ct);
        if (alertConfig is null) return;

        var vehicles = await _vehicleRepo.GetActiveVehiclesAsync(ct);
        foreach (var vehicle in vehicles)
        {
            if (!await _schedulingService.IsMaintenanceDueSoonAsync(vehicle.Prcoid, ct)) continue;

            var alreadyExists = await _alertRepo.ExistsUnresolvedForReferenceAsync(
                alertConfig.Alcoid, prcoid: vehicle.Prcoid, ct: ct);
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
        var alertConfig = await _alertRepo.GetConfigByTypeAsync(AlertTypes.LotePorVencer, ct);
        if (alertConfig is null) return;

        var daysThreshold = int.TryParse(alertConfig.ThresholdValue, out var days) ? days : 30;
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));

        var expiringLots = await _inventoryRepo.GetExpiringLotsAsync(limitDate, ct);
        foreach (var lot in expiringLots)
        {
            var alreadyExists = await _alertRepo.ExistsUnresolvedForReferenceAsync(
                alertConfig.Alcoid, mateid: lot.Mateid, maloid: lot.Maloid, ct: ct);
            if (alreadyExists) continue;

            var material = await _inventoryRepo.GetMaterialByIdAsync(lot.Mateid, ct);
            var message = $"Lote de {material?.Name} vence el {lot.ExpirationDate:dd/MM/yyyy}. " +
                          $"Cantidad restante: {lot.CurrentQuantity} {material?.UnitOfMeasure}.";
            var alert = AlertLog.Create(alertConfig.Alcoid, message,
                mateid: lot.Mateid, maloid: lot.Maloid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }

    private async Task CheckLowStockAsync(CancellationToken ct)
    {
        var alertConfig = await _alertRepo.GetConfigByTypeAsync(AlertTypes.StockBajo, ct);
        if (alertConfig is null) return;

        var lowStockMaterials = await _inventoryRepo.GetLowStockMaterialsAsync(ct);
        foreach (var material in lowStockMaterials)
        {
            var alreadyExists = await _alertRepo.ExistsUnresolvedForReferenceAsync(
                alertConfig.Alcoid, mateid: material.Mateid, ct: ct);
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
        var alertConfig = await _alertRepo.GetConfigByTypeAsync(AlertTypes.ComponentePorCaducar, ct);
        if (alertConfig is null) return;

        var daysThreshold = int.TryParse(alertConfig.ThresholdValue, out var days) ? days : 30;
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));

        var expiringComponents = await _inventoryRepo.GetExpiringComponentsAsync(limitDate, ct);
        foreach (var component in expiringComponents)
        {
            var alreadyExists = await _alertRepo.ExistsUnresolvedForReferenceAsync(
                alertConfig.Alcoid, prcoid: component.Prcoid, incoid: component.Incoid, ct: ct);
            if (alreadyExists) continue;

            var actionCatalog = component.ActionCatalog;
            var message = $"Componente '{actionCatalog?.Name}' en vehículo {component.Prcoid} " +
                          $"caduca el {component.ExpirationDate:dd/MM/yyyy}.";
            var alert = AlertLog.Create(alertConfig.Alcoid, message,
                prcoid: component.Prcoid, incoid: component.Incoid);
            await _alertRepo.AddAsync(alert, ct);
        }
    }
}