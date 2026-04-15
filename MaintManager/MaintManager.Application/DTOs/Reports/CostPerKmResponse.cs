namespace MaintManager.Application.DTOs.Reports;

public sealed record CostPerKmResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int TotalServices,
    decimal TotalMaterialCost,
    int CurrentKm,
    decimal CostPerKm
);

/// <summary>Tasa de emergencias por vehículo.</summary>
