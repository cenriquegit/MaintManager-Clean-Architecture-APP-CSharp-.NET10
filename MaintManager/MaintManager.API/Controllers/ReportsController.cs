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
            .Include(m => m.MaterialConsumptions)
            .Include(m => m.InstalledComponents).ThenInclude(c => c.ActionCatalog)
            .FirstOrDefaultAsync(m => m.Mainid == id, ct);

        if (maintenance is null)
            return NotFound(ApiResponse<object>.Fail("Orden de mantenimiento no encontrada."));

        var vehicle = await _context.Vehicles
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Prcoid == maintenance.Prcoid, ct);

        var vehicleLine = vehicle is not null
            ? $"{vehicle.Product?.Name ?? "?"} — {vehicle.LicensePlateNumber ?? "?"}"
            : "Vehículo no encontrado";

        var statusLabel = maintenance.Statid switch
        {
            "AC" => "Activo",
            "FI" => "Finalizado",
            "CA" => "Cancelado",
            _ => maintenance.Statid
        };

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
                    col.Item().Text($"N° {maintenance.OrderNumber ?? id.ToString()}  |  {statusLabel}").FontSize(11);
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text("Datos del Vehículo").Bold().FontSize(13);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text("Vehículo:"); table.Cell().Text(vehicleLine);
                        if (vehicle is not null)
                        {
                            table.Cell().Text("Km actuales:"); table.Cell().Text($"{vehicle.Mileage:N0} km");
                        }
                    });

                    col.Item().Text("Datos del Servicio").Bold().FontSize(13);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text("Tipo:"); table.Cell().Text(maintenance.MaintenanceType?.Name ?? "-");
                        table.Cell().Text("Servicio:"); table.Cell().Text(maintenance.ServiceType?.Name ?? "N/A");
                        table.Cell().Text("Fecha:"); table.Cell().Text(maintenance.MaintenanceDate.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Text("Km:"); table.Cell().Text($"{maintenance.Mileage:N0} km");
                        table.Cell().Text("Origen:"); table.Cell().Text(maintenance.OriginService);
                        table.Cell().Text("Nota:"); table.Cell().Text(maintenance.Note ?? "-");
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
                                h.Cell().Background("#1565C0").Text("Completado").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Producto").FontColor("#FFFFFF").Bold();
                            });
                            foreach (var detail in maintenance.ActionDetails)
                            {
                                table.Cell().Text(detail.ActionCatalog?.Name ?? "-");
                                table.Cell().Text(detail.Completed ? "Sí" : "No");
                                table.Cell().Text(detail.ProductUsed ?? "-");
                            }
                        });
                    }
                    if (maintenance.MaterialConsumptions.Any())
                    {
                        col.Item().Text("Materiales Consumidos").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            table.Header(h =>
                            {
                                h.Cell().Background("#1565C0").Text("Material (ID)").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Cantidad").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Origen").FontColor("#FFFFFF").Bold();
                            });
                            foreach (var c in maintenance.MaterialConsumptions)
                            {
                                table.Cell().Text($"#{c.Mateid}");
                                table.Cell().Text($"{c.Quantity}");
                                table.Cell().Text(c.Origin ?? "-");
                            }
                        });
                    }
                    if (maintenance.InstalledComponents.Any())
                    {
                        col.Item().Text("Componentes Instalados").Bold().FontSize(13);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(); c.RelativeColumn(); });
                            table.Header(h =>
                            {
                                h.Cell().Background("#1565C0").Text("Componente").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Km instalación").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1565C0").Text("Vence").FontColor("#FFFFFF").Bold();
                            });
                            foreach (var c in maintenance.InstalledComponents.Where(c => c.Active))
                            {
                                table.Cell().Text(c.ActionCatalog?.Name ?? "-");
                                table.Cell().Text($"{c.InstallationKm:N0}");
                                table.Cell().Text(c.ExpirationDate?.ToString("dd/MM/yyyy") ?? "N/A");
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
                        if (maintenance.Diagnosis.FutureRecommendations is not null)
                            col.Item().Text($"Recomendaciones: {maintenance.Diagnosis.FutureRecommendations}");
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

    [HttpPost("maintenance-orders")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportMaintenanceOrders(
        [FromBody] MaintenanceOrdersFilter filter, CancellationToken ct)
    {
        var query = _context.Maintenances
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .AsNoTracking();

        if (filter.DateFrom.HasValue)
            query = query.Where(m => m.MaintenanceDate >= filter.DateFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.DateTo.HasValue)
            query = query.Where(m => m.MaintenanceDate <= filter.DateTo.Value.ToDateTime(TimeOnly.MaxValue));
        if (filter.Prcoid.HasValue)
            query = query.Where(m => m.Prcoid == filter.Prcoid.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(m => m.Statid == filter.Status);
        if (filter.Matyid.HasValue)
            query = query.Where(m => m.Matyid == filter.Matyid.Value);

        var list = await query.OrderByDescending(m => m.MaintenanceDate).ToListAsync(ct);
        var workers = await _context.Workers.Include(w => w.Person).ToDictionaryAsync(w => w.Workid, w => w.Person?.Name ?? "?", ct);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Column(col =>
                {
                    col.Item().Text("Neo Plus Business S.A.C.").Bold().FontSize(14).FontColor("#1565C0");
                    col.Item().Text("Reporte de Órdenes de Mantenimiento").FontSize(12).FontColor("#546E8A");
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn();
                        c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(2);
                        c.RelativeColumn(2); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Background("#1565C0").Padding(4).Text("Placa").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Vehículo").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Tipo").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Servicio").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Fecha").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Km").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Técnico").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Estado").FontColor("#FFFFFF").Bold();
                    });
                    foreach (var m in list)
                    {
                        var vehicle = _context.Vehicles.Include(v => v.Product)
                            .FirstOrDefault(v => v.Prcoid == m.Prcoid);
                        table.Cell().Padding(2).Text(vehicle?.LicensePlateNumber ?? "-");
                        table.Cell().Padding(2).Text(vehicle?.Product?.Name ?? "-");
                        table.Cell().Padding(2).Text(m.MaintenanceType?.Name ?? "-");
                        table.Cell().Padding(2).Text(m.ServiceType?.Name ?? "N/A");
                        table.Cell().Padding(2).Text(m.MaintenanceDate.ToString("dd/MM/yy"));
                        table.Cell().Padding(2).Text($"{m.Mileage:N0}");
                        table.Cell().Padding(2).Text(m.AssignedTo > 0 ? workers.GetValueOrDefault(m.AssignedTo, "?") : "Sin asignar");
                        table.Cell().Padding(2).Text(m.Statid switch { "AC" => "Activo", "FI" => "Finalizado", "CA" => "Cancelado", _ => m.Statid });
                    }
                });
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Total: ").Bold(); t.Span($"{list.Count} órdenes"); });
                    row.RelativeItem().AlignRight().Text(t => { t.Span("Página "); t.CurrentPageNumber(); t.Span(" de "); t.TotalPages(); });
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"ordenes_mantenimiento_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpPost("alerts")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAlerts(
        [FromBody] AlertsFilter filter, CancellationToken ct)
    {
        var query = _context.AlertLogs
            .Include(a => a.AlertConfig)
            .AsNoTracking();

        if (filter.DateFrom.HasValue)
            query = query.Where(a => a.AlertDate >= filter.DateFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.DateTo.HasValue)
            query = query.Where(a => a.AlertDate <= filter.DateTo.Value.ToDateTime(TimeOnly.MaxValue));
        if (filter.Resolved.HasValue)
            query = query.Where(a => a.Resolved == filter.Resolved.Value);
        if (!string.IsNullOrWhiteSpace(filter.AlertType))
            query = query.Where(a => a.AlertConfig!.AlertType == filter.AlertType);

        var list = await query.OrderByDescending(a => a.AlertDate).ToListAsync(ct);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Column(col =>
                {
                    col.Item().Text("Neo Plus Business S.A.C.").Bold().FontSize(14).FontColor("#1565C0");
                    col.Item().Text("Reporte de Alertas del Sistema").FontSize(12).FontColor("#546E8A");
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(); c.RelativeColumn(2); c.RelativeColumn(3);
                        c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn();
                    });
                    table.Header(h =>
                    {
                        h.Cell().Background("#1565C0").Padding(4).Text("Fecha").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Tipo").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Mensaje").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Vehículo/Material").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Leída").FontColor("#FFFFFF").Bold();
                        h.Cell().Background("#1565C0").Padding(4).Text("Resuelta").FontColor("#FFFFFF").Bold();
                    });
                    foreach (var a in list)
                    {
                        var refText = a.Prcoid.HasValue ? $"Vehículo #{a.Prcoid}" : a.Mateid.HasValue ? $"Material #{a.Mateid}" : "-";
                        table.Cell().Padding(2).Text(a.AlertDate.ToString("dd/MM/yy HH:mm"));
                        table.Cell().Padding(2).Text(a.AlertConfig?.Description ?? a.AlertConfig?.AlertType ?? "-");
                        table.Cell().Padding(2).Text(a.Message ?? "-");
                        table.Cell().Padding(2).Text(refText);
                        table.Cell().Padding(2).Text(a.Read ? "Sí" : "No");
                        table.Cell().Padding(2).Text(a.Resolved ? "Sí" : "No");
                    }
                });
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Total: ").Bold(); t.Span($"{list.Count} alertas"); });
                    row.RelativeItem().AlignRight().Text(t => { t.Span("Página "); t.CurrentPageNumber(); t.Span(" de "); t.TotalPages(); });
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"alertas_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpPost("vehicle-history")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportVehicleHistory(
        [FromBody] VehicleHistoryFilter filter, CancellationToken ct)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Prcoid == filter.Prcoid, ct);
        if (vehicle is null)
            return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        var query = _context.Maintenances
            .Include(m => m.MaintenanceType)
            .Include(m => m.ServiceType)
            .Include(m => m.ActionDetails).ThenInclude(d => d.ActionCatalog)
            .Include(m => m.MaterialConsumptions)
            .Include(m => m.InstalledComponents).ThenInclude(c => c.ActionCatalog)
            .Include(m => m.Diagnosis)
            .Where(m => m.Prcoid == filter.Prcoid);

        if (filter.DateFrom.HasValue)
            query = query.Where(m => m.MaintenanceDate >= filter.DateFrom.Value.ToDateTime(TimeOnly.MinValue));
        if (filter.DateTo.HasValue)
            query = query.Where(m => m.MaintenanceDate <= filter.DateTo.Value.ToDateTime(TimeOnly.MaxValue));

        var orders = await query.OrderByDescending(m => m.MaintenanceDate).ToListAsync(ct);
        var workersVH = await _context.Workers.Include(w => w.Person).ToDictionaryAsync(w => w.Workid, w => w.Person?.Name ?? "?", ct);

        var vehicleLine = $"{vehicle.Product?.Name ?? "?"} — {vehicle.LicensePlateNumber ?? "?"}";

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Column(col =>
                {
                    col.Item().Text("Neo Plus Business S.A.C.").Bold().FontSize(14).FontColor("#1565C0");
                    col.Item().Text("Historial de Mantenimiento por Unidad").FontSize(12).FontColor("#546E8A");
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");
                });
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Vehículo: {vehicleLine}").Bold().FontSize(12);
                    col.Item().Text($"Período: {filter.DateFrom?.ToString("dd/MM/yyyy") ?? "Inicio"} - {filter.DateTo?.ToString("dd/MM/yyyy") ?? "Actual"}").FontSize(10).FontColor("#546E8A");
                    col.Item().LineHorizontal(1).LineColor("#CBD5E1");

                    if (orders.Count == 0)
                    {
                        col.Item().Text("No se encontraron órdenes para este vehículo en el período seleccionado.").FontSize(10).FontColor("#C62828");
                    }

                    foreach (var m in orders)
                    {
                        col.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#1565C0");

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Orden del {m.MaintenanceDate:dd/MM/yyyy} — {m.MaintenanceType?.Name ?? "-"}").Bold().FontSize(11);
                            row.ConstantItem(100).Text(m.Statid switch { "AC" => "ACTIVO", "FI" => "FINALIZADO", "CA" => "CANCELADO", _ => m.Statid }).FontSize(10).FontColor("#1565C0").Bold().AlignRight();
                        });
                        col.Item().Text($"Km: {m.Mileage:N0}  |  Técnico: {(m.AssignedTo > 0 ? workersVH.GetValueOrDefault(m.AssignedTo, "?") : "Sin asignar")}  |  Servicio: {m.ServiceType?.Name ?? "N/A"}").FontSize(9).FontColor("#546E8A");

                        if (m.ActionDetails.Any())
                        {
                            col.Item().Text("Acciones realizadas:").Bold().FontSize(9);
                            foreach (var d in m.ActionDetails)
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(15).Text(d.Completed ? "☑" : "☐").FontSize(9);
                                    row.AutoItem().Text(d.ActionCatalog?.Name ?? "-").FontSize(9);
                                });
                        }

                        if (m.MaterialConsumptions.Any())
                        {
                            col.Item().Text("Materiales consumidos:").Bold().FontSize(9);
                            foreach (var mc in m.MaterialConsumptions)
                            {
                                var mate = _context.Materials.Find(mc.Mateid);
                                col.Item().Text($"  • {mate?.Name ?? $"ID {mc.Mateid}"}: {mc.Quantity} {mate?.UnitOfMeasure ?? ""}").FontSize(9);
                            }
                        }

                        if (m.InstalledComponents.Any(c => c.Active))
                        {
                            col.Item().Text("Componentes instalados:").Bold().FontSize(9);
                            foreach (var ic in m.InstalledComponents.Where(c => c.Active))
                                col.Item().Text($"  • {ic.ActionCatalog?.Name ?? "-"} (km: {ic.InstallationKm:N0})").FontSize(9);
                        }

                        if (m.Diagnosis is not null)
                        {
                            col.Item().Text($"Diagnóstico: {m.Diagnosis.GeneralStatus} — Operativo: {(m.Diagnosis.VehicleOperative ? "Sí" : "No")}").FontSize(9);
                            if (m.Diagnosis.Observations is not null)
                                col.Item().Text($"Obs: {m.Diagnosis.Observations}").FontSize(9);
                        }
                    }
                });
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(t => { t.Span("Total: ").Bold(); t.Span($"{orders.Count} órdenes"); });
                    row.RelativeItem().AlignRight().Text(t => { t.Span("Página "); t.CurrentPageNumber(); t.Span(" de "); t.TotalPages(); });
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"historial_{vehicle.LicensePlateNumber ?? filter.Prcoid.ToString()}_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
