using MaintManager.Application.DTOs.Common;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
[Produces("application/json")]
public sealed class AgendaController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;

    public AgendaController(FleetMaintenanceContext context) => _context = context;

    [HttpGet("agenda")]
    public async Task<IActionResult> GetAgenda(CancellationToken ct)
    {
        var managedVehicles = await _context.ManagedVehicles
            .AsNoTracking()
            .Where(v => v.Source == "legacy" && v.Status)
            .ToListAsync(ct);

        var activeOrderPrcoids = await _context.Maintenances
            .AsNoTracking()
            .Where(m => m.Statid == "AC")
            .Select(m => m.Prcoid)
            .Distinct()
            .ToListAsync(ct);

        var activeOrders = await _context.Maintenances
            .AsNoTracking()
            .Where(m => m.Statid == "AC")
            .Select(m => new { m.Mainid, m.Prcoid, m.Matyid })
            .ToListAsync(ct);

        var schedules = await _context.VehicleSchedules
            .AsNoTracking()
            .Where(s => s.Status)
            .ToListAsync(ct);

        var overdue = new List<AgendaItem>();
        var upcoming = new List<AgendaItem>();
        var inService = new List<AgendaItem>();
        var ok = new List<AgendaItem>();

        foreach (var v in managedVehicles)
        {
            if (activeOrderPrcoids.Contains(v.Prcoid ?? 0))
            {
                var order = activeOrders.FirstOrDefault(o => o.Prcoid == v.Prcoid);
                inService.Add(new AgendaItem(
                    v.MvId, v.Prcoid ?? 0, v.LicensePlate, v.VehicleName ?? "-",
                    v.Brand, v.Model, v.Year,
                    order?.Mainid ?? 0, order?.Matyid == 2 ? "Emergencia" : "Servicio",
                    GetCurrentKm(v.Prcoid ?? 0), null, null));
                continue;
            }

            var schedule = schedules.FirstOrDefault(s => s.Prcoid == v.Prcoid);
            if (schedule is null)
            {
                ok.Add(new AgendaItem(v.MvId, v.Prcoid ?? 0, v.LicensePlate, v.VehicleName ?? "-",
                    v.Brand, v.Model, v.Year, 0, null, null, null, null));
                continue;
            }

            var currentKm = GetCurrentKm(v.Prcoid ?? 0);
            var diff = schedule.NextKm - currentKm;
            var alertKm = schedule.AlertKmThreshold ?? 800;

            if (currentKm >= schedule.NextKm)
                overdue.Add(new AgendaItem(v.MvId, v.Prcoid ?? 0, v.LicensePlate, v.VehicleName ?? "-",
                    v.Brand, v.Model, v.Year, 0, null, currentKm, schedule.NextKm, diff));
            else if (diff <= alertKm)
                upcoming.Add(new AgendaItem(v.MvId, v.Prcoid ?? 0, v.LicensePlate, v.VehicleName ?? "-",
                    v.Brand, v.Model, v.Year, 0, null, currentKm, schedule.NextKm, diff));
            else
                ok.Add(new AgendaItem(v.MvId, v.Prcoid ?? 0, v.LicensePlate, v.VehicleName ?? "-",
                    v.Brand, v.Model, v.Year, 0, null, currentKm, schedule.NextKm, diff));
        }

        var response = new AgendaResponse(overdue, upcoming, inService, ok);
        return Ok(ApiResponse<AgendaResponse>.Ok(response));
    }

    private int? GetCurrentKm(int prcoid) =>
        _context.Vehicles.AsNoTracking()
            .Where(pv => pv.Prcoid == prcoid)
            .Select(pv => pv.Mileage)
            .FirstOrDefault();
}

public sealed record AgendaResponse(
    IReadOnlyList<AgendaItem> Overdue,
    IReadOnlyList<AgendaItem> Upcoming,
    IReadOnlyList<AgendaItem> InService,
    IReadOnlyList<AgendaItem> Ok);

public sealed record AgendaItem(
    int MvId, int Prcoid, string Plate, string VehicleName,
    string? Brand, string? Model, short? Year,
    int OrderId, string? ServiceType,
    int? CurrentKm, int? NextKm, int? KmDiff);
