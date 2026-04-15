namespace MaintManager.Application.DTOs.Maintenance;

public sealed record MaintenanceListItem(
    int Mainid,
    string? LicensePlate,
    string VehicleName,
    string MaintenanceType,
    string? ServiceType,
    DateTime MaintenanceDate,
    int Mileage,
    string AssignedToName,
    string Status
);

/// <summary>Completar una acción del checklist.</summary>
