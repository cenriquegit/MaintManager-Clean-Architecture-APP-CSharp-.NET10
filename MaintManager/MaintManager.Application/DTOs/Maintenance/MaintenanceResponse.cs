namespace MaintManager.Application.DTOs.Maintenance;

public sealed record MaintenanceResponse(
    int Mainid,
    int Prcoid,
    string? LicensePlate,
    string VehicleName,
    string MaintenanceType,
    string? ServiceType,
    string? OrderNumber,
    DateTime MaintenanceDate,
    int Mileage,
    int? KmSinceLast,
    string? OilBrand,
    string? OilViscositySae,
    string? ClimateSeason,
    bool ShowOilInNextMaintenance,
    string OriginService,
    bool? IsEmergencyComplete,
    string AssignedToName,
    string RegisteredByName,
    string? Note,
    string Status,
    IReadOnlyList<ActionDetailResponse> ActionDetails,
    DiagnosisResponse? Diagnosis,
    IReadOnlyList<ComponentResponse> Components,
    IReadOnlyList<int> AllowedActionIds,
    IReadOnlyList<int> AllowedMaterialIds,
    IReadOnlyList<int> AllowedComponentIds
);

/// <summary>Elemento de lista de mantenimientos.</summary>
