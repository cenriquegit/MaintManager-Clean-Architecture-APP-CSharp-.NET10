namespace MaintManager.Application.DTOs.Vehicle;

public sealed record VehicleResponse(
    int Prcoid,
    string? LicensePlate,
    string? VinNumber,
    string VehicleName,
    string? VehicleType,
    string? FuelType,
    short? Year,
    string? Color,
    string? Category,
    int CurrentKm,
    int? NextMaintenanceKm,
    int? KmUntilNextService,
    bool IsMaintenanceDueSoon,
    bool IsActive
);

/// <summary>Datos simplificados para la lista de vehículos.</summary>
