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
                SELECT
                    (SELECT count(*) FROM product.vehicle WHERE status = true) AS ""TotalVehicles"",
                    (SELECT count(*) FROM maintenance.maintenance
                     WHERE statid = 'FI' AND maintenance_date >= date_trunc('month', CURRENT_DATE)) AS ""ServicesThisMonth"",
                    (SELECT round(((count(*) FILTER (WHERE mt.name = 'Emergencia'))::numeric
                        / NULLIF(count(*), 0)::numeric) * 100, 2)
                     FROM maintenance.maintenance m
                     JOIN maintenance.maintenance_type mt ON m.matyid = mt.matyid
                     WHERE m.statid = 'FI') AS ""GlobalEmergencyRatePercent"",
                    (SELECT count(*) FROM maintenance.vw_low_stock) AS ""LowStockMaterials"",
                    (SELECT count(*) FROM maintenance.alert_log WHERE resolved = false) AS ""UnresolvedAlerts"",
                    (SELECT count(*) FROM maintenance.vw_expiring_lots) AS ""ExpiringLots"",
                    (SELECT round(COALESCE(avg(cost_per_km), 0), 4)
                     FROM (
                         SELECT CASE WHEN vk.current_km > 0
                             THEN round(COALESCE(mc.total_material_cost, 0) / vk.current_km, 4)
                             ELSE 0 END AS cost_per_km
                         FROM maintenance.vw_vehicle_current_km vk
                         LEFT JOIN LATERAL (
                             SELECT m.prcoid, COALESCE(sum(mc_cost.cost_total), 0) AS total_material_cost
                             FROM maintenance.maintenance m
                             LEFT JOIN LATERAL (
                                 SELECT sum(mc.quantity * COALESCE(ml.unit_cost, 0)) AS cost_total
                                 FROM maintenance.material_consumption mc
                                 LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
                                 WHERE mc.mainid = m.mainid AND mc.origin = 'Stock propio'
                             ) mc_cost ON true
                             WHERE m.prcoid = vk.prcoid AND m.statid = 'FI'
                             GROUP BY m.prcoid
                         ) mc ON true
                     ) sub WHERE cost_per_km > 0) AS ""FleetAvgCostPerKm""
                ")
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
                SELECT vk.prcoid AS ""Prcoid"",
                       COALESCE(vk.license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vk.vehicle_name, '') AS ""VehicleName"",
                       COALESCE(mc.total_services, 0) AS ""TotalServices"",
                       COALESCE(mc.total_material_cost, 0) AS ""TotalMaterialCost"",
                       COALESCE(vk.current_km, 0) AS ""CurrentKm"",
                       CASE
                           WHEN vk.current_km > 0 THEN round(COALESCE(mc.total_material_cost, 0) / vk.current_km, 4)
                           ELSE 0
                       END AS ""CostPerKm""
                FROM maintenance.vw_vehicle_current_km vk
                LEFT JOIN LATERAL (
                    SELECT m.prcoid,
                           count(m.mainid) AS total_services,
                           COALESCE(sum(mc_cost.cost_total), 0) AS total_material_cost
                    FROM maintenance.maintenance m
                    LEFT JOIN LATERAL (
                        SELECT sum(mc.quantity * COALESCE(ml.unit_cost, 0)) AS cost_total
                        FROM maintenance.material_consumption mc
                        LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
                        WHERE mc.mainid = m.mainid AND mc.origin = 'Stock propio'
                    ) mc_cost ON true
                    WHERE m.prcoid = vk.prcoid AND m.statid = 'FI'
                    GROUP BY m.prcoid
                ) mc ON true
                ORDER BY ""CostPerKm"" DESC")
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
                SELECT m.prcoid AS ""Prcoid"",
                       COALESCE(vk.license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vk.vehicle_name, '') AS ""VehicleName"",
                       count(*) FILTER (WHERE mt.name = 'Calendarizado') AS ""ScheduledCount"",
                       count(*) FILTER (WHERE mt.name = 'Emergencia') AS ""EmergencyCount"",
                       count(*) AS ""TotalCount"",
                       CASE
                           WHEN count(*) > 0 THEN round((count(*) FILTER (WHERE mt.name = 'Emergencia')::numeric / count(*)::numeric) * 100, 2)
                           ELSE 0
                       END AS ""EmergencyRatePercent""
                FROM maintenance.maintenance m
                JOIN maintenance.maintenance_type mt ON m.matyid = mt.matyid
                LEFT JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
                WHERE m.statid = 'FI'
                GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name
                ORDER BY ""EmergencyRatePercent"" DESC")
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
                SELECT m.prcoid AS ""Prcoid"",
                       COALESCE(vk.license_plate_number, '') AS ""LicensePlate"",
                       COALESCE(vk.vehicle_name, '') AS ""VehicleName"",
                       m.mainid AS ""Mainid"",
                       m.maintenance_date AS ""MaintenanceDate"",
                       m.mileage AS ""ServiceKm"",
                       COALESCE(vs.next_km - vs.interval_km, 0) AS ""ScheduledKm"",
                       COALESCE(m.mileage - (vs.next_km - vs.interval_km), 0) AS ""KmDeviation"",
                       CASE
                           WHEN abs(m.mileage - (vs.next_km - vs.interval_km)) <= 500 THEN 'Puntual'
                           WHEN m.mileage < (vs.next_km - vs.interval_km) - 500 THEN 'Anticipado'
                           ELSE 'Tardio'
                       END AS ""ComplianceStatus""
                FROM maintenance.maintenance m
                JOIN maintenance.maintenance_type mt ON m.matyid = mt.matyid
                JOIN maintenance.vehicle_schedule vs ON m.prcoid = vs.prcoid
                LEFT JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
                WHERE m.statid = 'FI' AND mt.name = 'Calendarizado'
                ORDER BY m.maintenance_date DESC LIMIT 50")
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
