namespace MaintManager.Application.DTOs.Reports;

public sealed record MonthlyCostResponse(
    DateTime Month,
    int Prcoid,
    string LicensePlate,
    int ServicesCount,
    decimal MonthlyCost
);

/// <summary>Lote próximo a vencer.</summary>
