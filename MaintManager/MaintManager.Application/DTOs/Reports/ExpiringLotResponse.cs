namespace MaintManager.Application.DTOs.Reports;

public sealed record ExpiringLotResponse(
    int Maloid,
    int Mateid,
    string MaterialName,
    string Category,
    decimal CurrentQuantity,
    string UnitOfMeasure,
    DateOnly ExpirationDate,
    int DaysUntilExpiry,
    decimal UnitCost,
    decimal AtRiskCost,
    string LotStatus
);

/// <summary>Cumplimiento del calendario de mantenimiento.</summary>
