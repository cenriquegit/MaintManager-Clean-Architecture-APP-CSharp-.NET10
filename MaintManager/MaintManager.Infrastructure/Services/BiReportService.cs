using MaintManager.Application.DTOs.Reports;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Services;

public sealed class BiReportService : IBiReportService
{
    private readonly FleetMaintenanceContext _context;

    public BiReportService(FleetMaintenanceContext context) => _context = context;

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var summary = await _context.Database
            .SqlQueryRaw<DashboardSummaryRaw>(@"
                SELECT total_vehicles AS ""TotalVehicles"",
                       services_this_month AS ""ServicesThisMonth"",
                       COALESCE(global_emergency_rate_percent, 0) AS ""GlobalEmergencyRatePercent"",
                       low_stock_materials AS ""LowStockMaterials"",
                       unresolved_alerts AS ""UnresolvedAlerts"",
                       expiring_lots AS ""ExpiringLots"",
                       COALESCE(fleet_avg_cost_per_km, 0) AS ""FleetAvgCostPerKm""
                FROM maintenance.vw_bi_dashboard_summary")
            .FirstOrDefaultAsync(ct);

        if (summary is null)
            return new DashboardSummaryResponse(0, 0, 0, 0, 0, 0, 0);

        return new DashboardSummaryResponse(
            summary.TotalVehicles, summary.ServicesThisMonth,
            summary.GlobalEmergencyRatePercent, summary.LowStockMaterials,
            summary.UnresolvedAlerts, summary.ExpiringLots,
            summary.FleetAvgCostPerKm);
    }

    public async Task<IReadOnlyList<CostPerKmResponse>> GetCostPerKmAsync(CancellationToken ct = default)
    {
        var data = await _context.Database
            .SqlQueryRaw<CostPerKmRaw>(@"
                SELECT prcoid AS ""Prcoid"",
                       COALESCE(license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vehicle_name, '') AS ""VehicleName"",
                       total_services AS ""TotalServices"",
                       COALESCE(total_material_cost, 0) AS ""TotalMaterialCost"",
                       COALESCE(current_km, 0) AS ""CurrentKm"",
                       COALESCE(cost_per_km, 0) AS ""CostPerKm""
                FROM maintenance.vw_cost_per_km ORDER BY cost_per_km DESC")
            .ToListAsync(ct);

        return data.Select(r => new CostPerKmResponse(
            r.Prcoid, r.LicensePlate, r.VehicleName, r.TotalServices,
            r.TotalMaterialCost, r.CurrentKm, r.CostPerKm
        )).ToList();
    }

    public async Task<IReadOnlyList<EmergencyRateResponse>> GetEmergencyRateAsync(CancellationToken ct = default)
    {
        var data = await _context.Database
            .SqlQueryRaw<EmergencyRateRaw>(@"
                SELECT prcoid AS ""Prcoid"",
                       COALESCE(license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vehicle_name, '') AS ""VehicleName"",
                       scheduled_count AS ""ScheduledCount"",
                       emergency_count AS ""EmergencyCount"",
                       total_count AS ""TotalCount"",
                       COALESCE(emergency_rate_percent, 0) AS ""EmergencyRatePercent""
                FROM maintenance.vw_emergency_rate ORDER BY emergency_rate_percent DESC")
            .ToListAsync(ct);

        return data.Select(r => new EmergencyRateResponse(
            r.Prcoid, r.LicensePlate, r.VehicleName, r.ScheduledCount,
            r.EmergencyCount, r.TotalCount, r.EmergencyRatePercent
        )).ToList();
    }

    public async Task<IReadOnlyList<MonthlyCostResponse>> GetMonthlyCostAsync(int months, CancellationToken ct = default)
    {
        var data = await _context.Database
            .SqlQueryRaw<MonthlyCostRaw>(@"
                SELECT month AS ""Month"",
                       prcoid AS ""Prcoid"",
                       COALESCE(license_plate_number, '') AS ""LicensePlate"",
                       services_count AS ""ServicesCount"",
                       COALESCE(monthly_cost, 0) AS ""MonthlyCost""
                FROM maintenance.vw_monthly_cost
                WHERE month >= date_trunc('month', CURRENT_DATE - ({0} || ' months')::interval)
                ORDER BY month DESC, monthly_cost DESC", months)
            .ToListAsync(ct);

        return data.Select(r => new MonthlyCostResponse(
            r.Month, r.Prcoid, r.LicensePlate, r.ServicesCount, r.MonthlyCost
        )).ToList();
    }

    public async Task<IReadOnlyList<CalendarComplianceResponse>> GetCalendarComplianceAsync(CancellationToken ct = default)
    {
        var data = await _context.Database
            .SqlQueryRaw<CalendarComplianceRaw>(@"
                SELECT prcoid AS ""Prcoid"",
                       COALESCE(license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vehicle_name, '') AS ""VehicleName"",
                       mainid AS ""Mainid"",
                       maintenance_date AS ""MaintenanceDate"",
                       service_km AS ""ServiceKm"",
                       COALESCE(scheduled_km, 0) AS ""ScheduledKm"",
                       COALESCE(km_deviation, 0) AS ""KmDeviation"",
                       COALESCE(compliance_status, '') AS ""ComplianceStatus""
                FROM maintenance.vw_calendar_compliance
                ORDER BY maintenance_date DESC LIMIT 50")
            .ToListAsync(ct);

        return data.Select(r => new CalendarComplianceResponse(
            r.Prcoid, r.LicensePlate, r.VehicleName, r.Mainid,
            r.MaintenanceDate, r.ServiceKm, r.ScheduledKm, r.KmDeviation, r.ComplianceStatus
        )).ToList();
    }

    public async Task<IReadOnlyList<ExpiringLotResponse>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default)
    {
        var limitDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysThreshold));
        var lots = await _context.MaterialLots.AsNoTracking()
            .Where(ml => ml.LotStatus == "activo" && ml.ExpirationDate.HasValue && ml.ExpirationDate.Value <= limitDate)
            .Include(ml => ml.Material).ThenInclude(m => m!.Category)
            .OrderBy(ml => ml.ExpirationDate)
            .ToListAsync(ct);

        return lots.Select(l =>
        {
            var daysUntil = (l.ExpirationDate!.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;
            return new ExpiringLotResponse(
                l.Maloid, l.Mateid, l.Material?.Name ?? string.Empty,
                l.Material?.Category?.Name ?? string.Empty, l.CurrentQuantity,
                l.Material?.UnitOfMeasure ?? string.Empty, l.ExpirationDate.Value,
                daysUntil, l.UnitCost, l.CurrentQuantity * l.UnitCost, l.LotStatus
            );
        }).ToList();
    }

    Task<object> IBiReportService.GetDashboardSummaryAsync(CancellationToken ct)
        => Task.FromResult<object>(GetDashboardSummaryAsync(ct).Result!);

    Task<IReadOnlyList<object>> IBiReportService.GetCostPerKmAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<object>>(GetCostPerKmAsync(ct).Result.Cast<object>().ToList());

    Task<IReadOnlyList<object>> IBiReportService.GetEmergencyRateAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<object>>(GetEmergencyRateAsync(ct).Result.Cast<object>().ToList());

    Task<IReadOnlyList<object>> IBiReportService.GetMonthlyCostAsync(int months, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<object>>(GetMonthlyCostAsync(months, ct).Result.Cast<object>().ToList());

    Task<IReadOnlyList<object>> IBiReportService.GetExpiringLotsAsync(int daysThreshold, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<object>>(GetExpiringLotsAsync(daysThreshold, ct).Result.Cast<object>().ToList());

    Task<IReadOnlyList<object>> IBiReportService.GetCalendarComplianceAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<object>>(GetCalendarComplianceAsync(ct).Result.Cast<object>().ToList());

    private sealed class DashboardSummaryRaw
    {
        public int TotalVehicles { get; init; }
        public int ServicesThisMonth { get; init; }
        public decimal GlobalEmergencyRatePercent { get; init; }
        public int LowStockMaterials { get; init; }
        public int UnresolvedAlerts { get; init; }
        public int ExpiringLots { get; init; }
        public decimal FleetAvgCostPerKm { get; init; }
    }

    private sealed class CostPerKmRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int TotalServices { get; init; }
        public decimal TotalMaterialCost { get; init; }
        public int CurrentKm { get; init; }
        public decimal CostPerKm { get; init; }
    }

    private sealed class EmergencyRateRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int ScheduledCount { get; init; }
        public int EmergencyCount { get; init; }
        public int TotalCount { get; init; }
        public decimal EmergencyRatePercent { get; init; }
    }

    private sealed class MonthlyCostRaw
    {
        public DateTime Month { get; init; }
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public int ServicesCount { get; init; }
        public decimal MonthlyCost { get; init; }
    }

    private sealed class CalendarComplianceRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int Mainid { get; init; }
        public DateTime MaintenanceDate { get; init; }
        public int ServiceKm { get; init; }
        public int ScheduledKm { get; init; }
        public int KmDeviation { get; init; }
        public string ComplianceStatus { get; init; } = string.Empty;
    }
}
