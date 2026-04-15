// MaintManager.Application/Mappings/AllMappings.cs
using MaintManager.Application.DTOs.Inventory;
using MaintManager.Application.DTOs.Maintenance;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Application.DTOs.Vehicle;
using MaintManager.Domain.Entities;
using MaintManager.Domain.Entities.Existing;

namespace MaintManager.Application.Mappings;

/// <summary>Mapeos de Vehículo → DTO.</summary>
public static class VehicleMappings
{
    public static VehicleListItem ToListItem(this Vehicle v, int currentKm,
        int? nextMaintenanceKm, bool isDueSoon) =>
        new(
            Prcoid: v.Prcoid,
            LicensePlate: v.LicensePlateNumber,
            VehicleName: v.Product?.Name ?? "Sin nombre",
            CurrentKm: currentKm,
            NextMaintenanceKm: nextMaintenanceKm,
            IsMaintenanceDueSoon: isDueSoon
        );

    public static VehicleResponse ToResponse(this Vehicle v, int currentKm,
        int? nextKm, int? alertThreshold, bool isDueSoon) =>
        new(
            Prcoid: v.Prcoid,
            LicensePlate: v.LicensePlateNumber,
            VinNumber: v.VinNumber,
            VehicleName: v.Product?.Name ?? "Sin nombre",
            VehicleType: v.Vetyid,
            FuelType: v.Futyid,
            Year: v.YearOfManufacture,
            Color: v.Color,
            Category: v.Category,
            CurrentKm: currentKm,
            NextMaintenanceKm: nextKm,
            KmUntilNextService: nextKm.HasValue ? nextKm.Value - currentKm : null,
            IsMaintenanceDueSoon: isDueSoon,
            IsActive: v.Status
        );
}

/// <summary>Mapeos de Maintenance → DTO.</summary>
public static class MaintenanceMappings
{
    public static MaintenanceListItem ToListItem(this Maintenance m,
        string licensePlate, string vehicleName, string assignedToName) =>
        new(
            Mainid: m.Mainid,
            LicensePlate: licensePlate,
            VehicleName: vehicleName,
            MaintenanceType: m.MaintenanceType?.Name ?? string.Empty,
            ServiceType: m.ServiceType?.Name,
            MaintenanceDate: m.MaintenanceDate,
            Mileage: m.Mileage,
            AssignedToName: assignedToName,
            Status: m.Statid
        );

    public static MaintenanceResponse ToResponse(this Maintenance m,
        string licensePlate, string vehicleName,
        string assignedToName, string registeredByName) =>
        new(
            Mainid: m.Mainid,
            Prcoid: m.Prcoid,
            LicensePlate: licensePlate,
            VehicleName: vehicleName,
            MaintenanceType: m.MaintenanceType?.Name ?? string.Empty,
            ServiceType: m.ServiceType?.Name,
            OrderNumber: m.OrderNumber,
            MaintenanceDate: m.MaintenanceDate,
            Mileage: m.Mileage,
            KmSinceLast: m.KmSinceLast,
            OilBrand: m.OilBrand,
            OilViscositySae: m.OilViscositySae,
            ClimateSeason: m.ClimateSeason,
            ShowOilInNextMaintenance: m.ShowOilInNextMaintenance,
            OriginService: m.OriginService,
            IsEmergencyComplete: m.IsEmergencyComplete,
            AssignedToName: assignedToName,
            RegisteredByName: registeredByName,
            Note: m.Note,
            Status: m.Statid,
            ActionDetails: m.ActionDetails.Select(d => d.ToResponse()).ToList(),
            Diagnosis: m.Diagnosis?.ToResponse()
        );

    public static ActionDetailResponse ToResponse(this MaintenanceActionDetail d) =>
        new(
            Madeid: d.Madeid,
            Acatid: d.Acatid,
            ActionName: d.ActionCatalog?.Name ?? string.Empty,
            ActionCategory: d.ActionCatalog?.Category ?? string.Empty,
            Completed: d.Completed,
            ActionPerformed: d.ActionPerformed,
            ProductUsed: d.ProductUsed,
            QuantityUsed: d.QuantityUsed,
            OriginProduct: d.OriginProduct,
            Observation: d.Observation
        );

    public static DiagnosisResponse ToResponse(this Diagnosis d) =>
        new(
            GeneralStatus: d.GeneralStatus,
            VehicleOperative: d.VehicleOperative,
            Observations: d.Observations,
            FutureRecommendations: d.FutureRecommendations,
            CreatedAt: d.CreatedAt
        );
}

/// <summary>Mapeos de inventario → DTO.</summary>
public static class InventoryMappings
{
    public static MaterialListItem ToListItem(this Material m) =>
        new(
            Mateid: m.Mateid,
            Category: m.Category?.Name ?? string.Empty,
            Name: m.Name,
            UnitOfMeasure: m.UnitOfMeasure,
            StockTotal: m.StockTotal,
            StockMinimum: m.StockMinimum,
            IsBelowMinimum: m.IsBelowMinimum()
        );

    public static MaterialResponse ToResponse(this Material m) =>
        new(
            Mateid: m.Mateid,
            Category: m.Category?.Name ?? string.Empty,
            Name: m.Name,
            UnitOfMeasure: m.UnitOfMeasure,
            StockTotal: m.StockTotal,
            StockMinimum: m.StockMinimum,
            IsBelowMinimum: m.IsBelowMinimum(),
            Description: m.Description,
            ActiveLots: m.Lots
                .Where(l => l.LotStatus == "activo")
                .Select(l => l.ToResponse())
                .ToList()
        );

    public static LotResponse ToResponse(this MaterialLot l)
    {
        int? daysUntilExpiry = l.ExpirationDate.HasValue
            ? (l.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days
            : null;

        return new LotResponse(
            Maloid: l.Maloid,
            InitialQuantity: l.InitialQuantity,
            CurrentQuantity: l.CurrentQuantity,
            UnitCost: l.UnitCost,
            EntryDate: l.EntryDate,
            ExpirationDate: l.ExpirationDate,
            DaysUntilExpiry: daysUntilExpiry,
            LotStatus: l.LotStatus
        );
    }

    public static AlertResponse ToAlertResponse(this AlertLog al) =>
        new(
            Alloid: al.Alloid,
            AlertType: al.AlertConfig?.AlertType ?? string.Empty,
            Message: al.Message,
            AlertDate: al.AlertDate,
            LicensePlate: null, // Se rellena en el servicio
            MaterialName: null, // Se rellena en el servicio
            IsRead: al.Read,
            IsResolved: al.Resolved
        );
}
