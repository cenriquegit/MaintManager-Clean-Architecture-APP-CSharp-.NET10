namespace MaintManager.Shared.Models;

public sealed record DashboardSummaryDto(
    int TotalVehicles,
    int ServicesThisMonth,
    decimal GlobalEmergencyRatePercent,
    int LowStockMaterials,
    int UnresolvedAlerts,
    int ExpiringLots,
    decimal FleetAvgCostPerKm
);
