namespace MaintManager.Application.DTOs.Reports;

public sealed record EmergencyRateResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int ScheduledCount,
    int EmergencyCount,
    int TotalCount,
    decimal EmergencyRatePercent
);

/// <summary>Costo mensual por vehículo.</summary>
