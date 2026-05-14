using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Domain.Interfaces.Services;
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

[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly IBiReportService _biReportService;
    private readonly FleetMaintenanceContext _context;

    public ReportsController(IBiReportService biReportService, FleetMaintenanceContext context)
    {
        _biReportService = biReportService;
        _context = context;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var raw = await _biReportService.GetDashboardSummaryAsync(ct);
        var summary = (DashboardSummaryResponse)raw;
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(summary));
    }

    [HttpGet("cost-per-km")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CostPerKmResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostPerKm(CancellationToken ct)
    {
        var raw = await _biReportService.GetCostPerKmAsync(ct);
        var result = raw.Cast<CostPerKmResponse>().ToList();
        return Ok(ApiResponse<IReadOnlyList<CostPerKmResponse>>.Ok(result));
    }

    [HttpGet("emergency-rate")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyRateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmergencyRate(CancellationToken ct)
    {
        var raw = await _biReportService.GetEmergencyRateAsync(ct);
        var result = raw.Cast<EmergencyRateResponse>().ToList();
        return Ok(ApiResponse<IReadOnlyList<EmergencyRateResponse>>.Ok(result));
    }

    [HttpGet("monthly-cost")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MonthlyCostResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyCost([FromQuery] int months = 6, CancellationToken ct = default)
    {
        var raw = await _biReportService.GetMonthlyCostAsync(months, ct);
        var result = raw.Cast<MonthlyCostResponse>().ToList();
        return Ok(ApiResponse<IReadOnlyList<MonthlyCostResponse>>.Ok(result));
    }

    [HttpGet("calendar-compliance")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CalendarComplianceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarCompliance(CancellationToken ct)
    {
        var raw = await _biReportService.GetCalendarComplianceAsync(ct);
        var result = raw.Cast<CalendarComplianceResponse>().ToList();
        return Ok(ApiResponse<IReadOnlyList<CalendarComplianceResponse>>.Ok(result));
    }

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
                    col.Item().Text("Neo Plus Business S.A.C.").Bold().FontSize(16).FontColor("#1565C0");
                    col.Item().Text("Orden de Mantenimiento Vehicular").FontSize(13).FontColor("#546E8A");
                    col.Item().Text($"N° {maintenance.OrderNumber ?? id.ToString()}").FontSize(11);
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text("Datos del Servicio").Bold().FontSize(13);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text("Tipo:"); table.Cell().Text(maintenance.MaintenanceType?.Name ?? "-");
                        table.Cell().Text("Servicio:"); table.Cell().Text(maintenance.ServiceType?.Name ?? "N/A");
                        table.Cell().Text("Fecha:"); table.Cell().Text(maintenance.MaintenanceDate.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Text("Km:"); table.Cell().Text($"{maintenance.Mileage:N0} km");
                        table.Cell().Text("Origen:"); table.Cell().Text(maintenance.OriginService);
                    });
                    if (maintenance.ActionDetails.Any())
                    {
                        col.Item().Text("Acciones Realizadas").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(2); });
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
                        col.Item().Text($"Operativo: {(maintenance.Diagnosis.VehicleOperative ? "Sí" : "No")}");
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
                    row.RelativeItem().Text(t => { t.Span("Generado: ").Bold(); t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")); });
                    row.RelativeItem().AlignRight().Text(t => { t.Span("Página "); t.CurrentPageNumber(); t.Span(" de "); t.TotalPages(); });
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"mantenimiento_{maintenance.OrderNumber ?? id.ToString()}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("cost-excel")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCostExcel(CancellationToken ct)
    {
        var raw = await _biReportService.GetCostPerKmAsync(ct);
        var data = raw.Cast<CostPerKmResponse>().ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Costo por Kilómetro");
        ws.Cell("A1").Value = "Neo Plus Business S.A.C. — Reporte de Costo por Km";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1565C0");
        ws.Range("A1:G1").Merge();
        ws.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range("A2:G2").Merge();

        var headers = new[] { "Placa", "Vehículo", "Total Servicios", "Costo Total Materiales (S/)", "Km Actual", "Costo/Km (S/)" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

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
