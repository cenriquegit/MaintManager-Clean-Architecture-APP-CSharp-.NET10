namespace MaintManager.Application.DTOs.Reports;

public sealed record DashboardSummaryResponse(
    int TotalVehicles,
    int ServicesThisMonth,
    decimal GlobalEmergencyRatePercent,
    int LowStockMaterials,
    int UnresolvedAlerts,
    int ExpiringLots,
    decimal FleetAvgCostPerKm
);

/// <summary>Costo por km por vehículo.</summary>
