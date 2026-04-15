using MaintManager.Application.DTOs.Maintenance;
using MaintManager.Domain.Entities;

namespace MaintManager.Application.Mappings;

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