// MaintManager.API/Controllers/ReportsController.cs
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;

namespace MaintManager.API.Controllers;

/// <summary>
/// Endpoints analíticos de Business Intelligence.
/// Consultan directamente las vistas BI de PostgreSQL para máximo rendimiento.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;

    public ReportsController(FleetMaintenanceContext context) => _context = context;

    /// <summary>KPIs del panel principal del dashboard BI.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        // Consultar la vista vw_bi_dashboard_summary
        var summary = await _context.Database
            .SqlQueryRaw<DashboardSummaryRaw>(@"
                SELECT
                    total_vehicles,
                    services_this_month,
                    COALESCE(global_emergency_rate_percent, 0) AS global_emergency_rate_percent,
                    low_stock_materials,
                    unresolved_alerts,
                    expiring_lots,
                    COALESCE(fleet_avg_cost_per_km, 0) AS fleet_avg_cost_per_km
                FROM maintenance.vw_bi_dashboard_summary")
            .FirstOrDefaultAsync(ct);

        if (summary is null)
            return Ok(ApiResponse<DashboardSummaryResponse>.Ok(
                new DashboardSummaryResponse(0, 0, 0, 0, 0, 0, 0)));

        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(new DashboardSummaryResponse(
            TotalVehicles: summary.TotalVehicles,
            ServicesThisMonth: summary.ServicesThisMonth,
            GlobalEmergencyRatePercent: summary.GlobalEmergencyRatePercent,
            LowStockMaterials: summary.LowStockMaterials,
            UnresolvedAlerts: summary.UnresolvedAlerts,
            ExpiringLots: summary.ExpiringLots,
            FleetAvgCostPerKm: summary.FleetAvgCostPerKm
        )));
    }

    /// <summary>Costo por kilómetro por vehículo. Para gráfico de barras.</summary>
    [HttpGet("cost-per-km")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CostPerKmResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostPerKm(CancellationToken ct)
    {
        var data = await _context.Database
            .SqlQueryRaw<CostPerKmRaw>(@"
                SELECT
                    prcoid,
                    license_plate_number AS license_plate,
                    vehicle_name,
                    total_services,
                    COALESCE(total_material_cost, 0) AS total_material_cost,
                    COALESCE(current_km, 0) AS current_km,
                    COALESCE(cost_per_km, 0) AS cost_per_km
                FROM maintenance.vw_cost_per_km
                ORDER BY cost_per_km DESC")
            .ToListAsync(ct);

        var result = data.Select(r => new CostPerKmResponse(
            Prcoid: r.Prcoid,
            LicensePlate: r.LicensePlate,
            VehicleName: r.VehicleName,
            TotalServices: r.TotalServices,
            TotalMaterialCost: r.TotalMaterialCost,
            CurrentKm: r.CurrentKm,
            CostPerKm: r.CostPerKm
        )).ToList();

        return Ok(ApiResponse<IReadOnlyList<CostPerKmResponse>>.Ok(result));
    }

    /// <summary>Tasa de emergencias por vehículo. Para gráfico de dona/Pareto.</summary>
    [HttpGet("emergency-rate")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyRateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmergencyRate(CancellationToken ct)
    {
        var data = await _context.Database
            .SqlQueryRaw<EmergencyRateRaw>(@"
                SELECT
                    prcoid,
                    license_plate_number AS license_plate,
                    vehicle_name,
                    scheduled_count,
                    emergency_count,
                    total_count,
                    COALESCE(emergency_rate_percent, 0) AS emergency_rate_percent
                FROM maintenance.vw_emergency_rate
                ORDER BY emergency_rate_percent DESC")
            .ToListAsync(ct);

        var result = data.Select(r => new EmergencyRateResponse(
            Prcoid: r.Prcoid,
            LicensePlate: r.LicensePlate,
            VehicleName: r.VehicleName,
            ScheduledCount: r.ScheduledCount,
            EmergencyCount: r.EmergencyCount,
            TotalCount: r.TotalCount,
            EmergencyRatePercent: r.EmergencyRatePercent
        )).ToList();

        return Ok(ApiResponse<IReadOnlyList<EmergencyRateResponse>>.Ok(result));
    }

    /// <summary>Costo mensual por vehículo. Para gráfico de líneas o barras apiladas.</summary>
    [HttpGet("monthly-cost")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MonthlyCostResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyCost(
        [FromQuery] int months = 6, CancellationToken ct = default)
    {
        var data = await _context.Database
            .SqlQueryRaw<MonthlyCostRaw>(@"
                SELECT
                    month,
                    prcoid,
                    license_plate_number AS license_plate,
                    services_count,
                    COALESCE(monthly_cost, 0) AS monthly_cost
                FROM maintenance.vw_monthly_cost
                WHERE month >= date_trunc('month', CURRENT_DATE - ({0} || ' months')::interval)
                ORDER BY month DESC, monthly_cost DESC", months)
            .ToListAsync(ct);

        var result = data.Select(r => new MonthlyCostResponse(
            Month: r.Month,
            Prcoid: r.Prcoid,
            LicensePlate: r.LicensePlate,
            ServicesCount: r.ServicesCount,
            MonthlyCost: r.MonthlyCost
        )).ToList();

        return Ok(ApiResponse<IReadOnlyList<MonthlyCostResponse>>.Ok(result));
    }

    /// <summary>Cumplimiento del calendario de mantenimiento.</summary>
    [HttpGet("calendar-compliance")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CalendarComplianceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarCompliance(CancellationToken ct)
    {
        var data = await _context.Database
            .SqlQueryRaw<CalendarComplianceRaw>(@"
                SELECT
                    prcoid,
                    license_plate_number AS license_plate,
                    vehicle_name,
                    mainid,
                    maintenance_date,
                    service_km,
                    COALESCE(scheduled_km, 0) AS scheduled_km,
                    COALESCE(km_deviation, 0) AS km_deviation,
                    compliance_status
                FROM maintenance.vw_calendar_compliance
                ORDER BY maintenance_date DESC
                LIMIT 50")
            .ToListAsync(ct);

        var result = data.Select(r => new CalendarComplianceResponse(
            Prcoid: r.Prcoid,
            LicensePlate: r.LicensePlate,
            VehicleName: r.VehicleName,
            Mainid: r.Mainid,
            MaintenanceDate: r.MaintenanceDate,
            ServiceKm: r.ServiceKm,
            ScheduledKm: r.ScheduledKm,
            KmDeviation: r.KmDeviation,
            ComplianceStatus: r.ComplianceStatus
        )).ToList();

        return Ok(ApiResponse<IReadOnlyList<CalendarComplianceResponse>>.Ok(result));
    }

    /// <summary>Exportar orden de mantenimiento a PDF.</summary>
    [HttpGet("maintenances/{id:int}/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMaintenancePdf(int id, CancellationToken ct)
    {
        var maintenance = await _context.Maintenances
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .Include(m => m.ActionDetails).ThenInclude(d => d.ActionCatalog)
            .Include(m => m.Diagnosis)
            .FirstOrDefaultAsync(m => m.Mainid == id, ct);

        if (maintenance is null)
            return NotFound(ApiResponse<object>.Fail("Orden de mantenimiento no encontrada."));

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Neo Plus Business S.A.C.")
                        .Bold().FontSize(16).FontColor("#1565C0");
                    col.Item().Text("Orden de Mantenimiento Vehicular")
                        .FontSize(13).FontColor("#546E8A");
                    col.Item().Text($"N° {maintenance.OrderNumber ?? id.ToString()}")
                        .FontSize(11);
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Datos del Servicio").Bold().FontSize(13);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text("Tipo de mantenimiento:");
                        table.Cell().Text(maintenance.MaintenanceType?.Name ?? "-");
                        table.Cell().Text("Tipo de servicio:");
                        table.Cell().Text(maintenance.ServiceType?.Name ?? "N/A");
                        table.Cell().Text("Fecha:");
                        table.Cell().Text(maintenance.MaintenanceDate.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Text("Kilometraje:");
                        table.Cell().Text($"{maintenance.Mileage:N0} km");
                        table.Cell().Text("Origen:");
                        table.Cell().Text(maintenance.OriginService);
                    });

                    if (maintenance.OilBrand is not null)
                    {
                        col.Item().Text("Información de Aceite").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            table.Cell().Text("Marca:");
                            table.Cell().Text(maintenance.OilBrand);
                            table.Cell().Text("Viscosidad SAE:");
                            table.Cell().Text(maintenance.OilViscositySae ?? "-");
                        });
                    }

                    if (maintenance.ActionDetails.Any())
                    {
                        col.Item().Text("Acciones Realizadas").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn();
                                c.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Background("#1565C0").Text("Acción").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Código").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Producto").FontColor("#FFFFFF").Bold();
                            });
                            foreach (var detail in maintenance.ActionDetails)
                            {
                                table.Cell().Text(detail.ActionCatalog?.Name ?? "-");
                                table.Cell().Text(detail.ActionPerformed?.ToString() ?? "-");
                                table.Cell().Text(detail.ProductUsed ?? "-");
                            }
                        });
                    }

                    if (maintenance.Diagnosis is not null)
                    {
                        col.Item().Text("Diagnóstico Final").Bold().FontSize(13);
                        col.Item().Text($"Estado: {maintenance.Diagnosis.GeneralStatus}");
                        col.Item().Text($"Vehículo operativo: {(maintenance.Diagnosis.VehicleOperative ? "Sí" : "No")}");
                        if (maintenance.Diagnosis.Observations is not null)
                            col.Item().Text($"Observaciones: {maintenance.Diagnosis.Observations}");
                    }

                    if (maintenance.Note is not null)
                    {
                        col.Item().Text("Notas").Bold().FontSize(13);
                        col.Item().Text(maintenance.Note);
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Generado: ").Bold();
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf",
            $"mantenimiento_{maintenance.OrderNumber ?? id.ToString()}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>Exportar reporte de costos a Excel.</summary>
    [HttpGet("cost-excel")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCostExcel(CancellationToken ct)
    {
        var data = await _context.Database
            .SqlQueryRaw<CostPerKmRaw>(@"
                SELECT prcoid, license_plate_number AS license_plate,
                       vehicle_name, total_services,
                       COALESCE(total_material_cost, 0) AS total_material_cost,
                       COALESCE(current_km, 0) AS current_km,
                       COALESCE(cost_per_km, 0) AS cost_per_km
                FROM maintenance.vw_cost_per_km
                ORDER BY cost_per_km DESC")
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Costo por Kilómetro");

        // Encabezado con estilo NeoPlus
        ws.Cell("A1").Value = "Neo Plus Business S.A.C. — Reporte de Costo por Km";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1565C0");
        ws.Range("A1:G1").Merge();

        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range("A2:G2").Merge();

        // Cabeceras
        var headers = new[]
        {
            "Placa", "Vehículo", "Total Servicios",
            "Costo Total Materiales (S/)", "Km Actual", "Costo/Km (S/)"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Datos
        for (var i = 0; i < data.Count; i++)
        {
            var row = data[i];
            var rowNum = i + 5;
            ws.Cell(rowNum, 1).Value = row.LicensePlate;
            ws.Cell(rowNum, 2).Value = row.VehicleName;
            ws.Cell(rowNum, 3).Value = row.TotalServices;
            ws.Cell(rowNum, 4).Value = (double)row.TotalMaterialCost;
            ws.Cell(rowNum, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(rowNum, 5).Value = row.CurrentKm;
            ws.Cell(rowNum, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(rowNum, 6).Value = (double)row.CostPerKm;
            ws.Cell(rowNum, 6).Style.NumberFormat.Format = "#,##0.0000";

            // Filas alternas
            if (i % 2 == 0)
                ws.Row(rowNum).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EEF7");
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reporte_costos_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Clases internas para raw SQL queries (proyecciones de vistas BI)
// ─────────────────────────────────────────────────────────────────────────────

file sealed class DashboardSummaryRaw
{
    public int TotalVehicles { get; init; }
    public int ServicesThisMonth { get; init; }
    public decimal GlobalEmergencyRatePercent { get; init; }
    public int LowStockMaterials { get; init; }
    public int UnresolvedAlerts { get; init; }
    public int ExpiringLots { get; init; }
    public decimal FleetAvgCostPerKm { get; init; }
}

file sealed class CostPerKmRaw
{
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleName { get; init; } = string.Empty;
    public int TotalServices { get; init; }
    public decimal TotalMaterialCost { get; init; }
    public int CurrentKm { get; init; }
    public decimal CostPerKm { get; init; }
}

file sealed class EmergencyRateRaw
{
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleName { get; init; } = string.Empty;
    public int ScheduledCount { get; init; }
    public int EmergencyCount { get; init; }
    public int TotalCount { get; init; }
    public decimal EmergencyRatePercent { get; init; }
}

file sealed class MonthlyCostRaw
{
    public DateTime Month { get; init; }
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public int ServicesCount { get; init; }
    public decimal MonthlyCost { get; init; }
}

file sealed class CalendarComplianceRaw
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
