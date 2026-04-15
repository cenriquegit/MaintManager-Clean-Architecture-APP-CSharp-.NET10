namespace MaintManager.Application.DTOs.Vehicle;

public sealed record VehicleListItem(
    int Prcoid,
    string? LicensePlate,
    string VehicleName,
    int CurrentKm,
    int? NextMaintenanceKm,
    bool IsMaintenanceDueSoon
);
