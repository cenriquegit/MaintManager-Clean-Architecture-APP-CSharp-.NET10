
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de Business Intelligence y reportes.</summary>
public interface IBiReportService
{
    Task<object> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetCostPerKmAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetEmergencyRateAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetMonthlyCostAsync(int months, CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetCalendarComplianceAsync(CancellationToken ct = default);
    Task<byte[]> ExportMaintenanceToPdfAsync(int mainid, CancellationToken ct = default);
    Task<byte[]> ExportCostReportToExcelAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

