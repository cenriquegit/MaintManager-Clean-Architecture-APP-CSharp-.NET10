using MaintManager.Application.DTOs.Common;
using MaintManager.Domain.Entities;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

[ApiController]
[Route("api/v1/vehicles/managed")]
[Authorize]
[Produces("application/json")]
public sealed class VehicleManagementController : ControllerBase
{
    private readonly IManagedVehicleRepository _mvRepo;
    private readonly ISunarpService _sunarp;
    private readonly FleetMaintenanceContext _context;

    public VehicleManagementController(IManagedVehicleRepository mvRepo, ISunarpService sunarp, FleetMaintenanceContext context)
    {
        _mvRepo = mvRepo;
        _sunarp = sunarp;
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ManagedVehicleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? source, CancellationToken ct)
    {
        var vehicles = await _mvRepo.GetAllAsync(search, source, ct);
        var prcoids = vehicles.Where(v => v.Prcoid.HasValue).Select(v => v.Prcoid!.Value).ToHashSet();

        var mileages = await _context.Vehicles.AsNoTracking()
            .Where(pv => prcoids.Contains(pv.Prcoid))
            .ToDictionaryAsync(pv => pv.Prcoid, pv => pv.Mileage, ct);

        var schedules = await _context.VehicleSchedules.AsNoTracking()
            .Where(s => prcoids.Contains(s.Prcoid) && s.Status)
            .ToDictionaryAsync(s => s.Prcoid, s => s.NextKm, ct);

        var activePrcoids = await _context.Maintenances.AsNoTracking()
            .Where(m => m.Statid == "AC" && prcoids.Contains(m.Prcoid))
            .Select(m => m.Prcoid).Distinct().ToHashSetAsync(ct);

        var dtos = vehicles.Select(v =>
        {
            var prcoid = v.Prcoid ?? 0;
            int? currentKm = prcoid > 0 && mileages.TryGetValue(prcoid, out var mk) ? mk : null;
            int? nextKm = prcoid > 0 && schedules.TryGetValue(prcoid, out var sk) ? sk : null;
            bool hasActive = prcoid > 0 && activePrcoids.Contains(prcoid);
            return new ManagedVehicleDto(v.MvId, v.Prcoid, v.LicensePlate, v.VehicleName,
                v.Brand, v.Model, v.Year, v.Color, v.Vin, v.EngineNumber,
                v.Source, v.Status, v.CreatedAt, v.UpdatedAt,
                currentKm, nextKm, hasActive);
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<ManagedVehicleDto>>.Ok(dtos));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ManagedVehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var v = await _mvRepo.GetByIdAsync(id, ct);
        if (v is null) return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));
        return Ok(ApiResponse<ManagedVehicleDto>.Ok(ToDto(v)));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ManagedVehicleDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateManagedVehicleRequest request, CancellationToken ct)
    {
        if (await _mvRepo.GetByPlateAsync(request.LicensePlate, ct) is not null)
            return BadRequest(ApiResponse<object>.Fail("Ya existe un vehículo con esa placa."));

        var vehicle = ManagedVehicle.Create(
            request.LicensePlate.ToUpper(),
            request.VehicleName,
            request.Brand, request.Model, request.Year,
            request.Color, request.Vin, request.EngineNumber,
            "managed");

        await _mvRepo.AddAsync(vehicle, ct);
        await _mvRepo.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = vehicle.MvId },
            ApiResponse<ManagedVehicleDto>.Ok(ToDto(vehicle)));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ManagedVehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManagedVehicleRequest request, CancellationToken ct)
    {
        var v = await _context.ManagedVehicles.FirstOrDefaultAsync(m => m.MvId == id, ct);
        if (v is null) return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        v.Update(request.VehicleName, request.Brand, request.Model,
            request.Year, request.Color, request.Vin, request.EngineNumber);

        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<ManagedVehicleDto>.Ok(ToDto(v)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var v = await _context.ManagedVehicles.FirstOrDefaultAsync(m => m.MvId == id, ct);
        if (v is null) return NotFound(ApiResponse<object>.Fail("Vehículo no encontrado."));

        _context.ManagedVehicles.Remove(v);
        await _context.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Vehículo eliminado." }));
    }

    [HttpPost("sunarp-captcha")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<SunarpCaptchaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSunarpCaptcha([FromQuery] string plate, CancellationToken ct)
    {
        var result = await _sunarp.GetCaptchaAsync(plate, ct);
        return Ok(ApiResponse<SunarpCaptchaDto>.Ok(new SunarpCaptchaDto(result.Success, result.CaptchaBase64, result.Error)));
    }

    [HttpPost("sunarp-consult")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<SunarpVehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConsultSunarp([FromBody] SunarpConsultRequest request, CancellationToken ct)
    {
        var result = await _sunarp.ConsultAsync(request.Plate, request.CaptchaCode, ct);
        return Ok(ApiResponse<SunarpVehicleDto>.Ok(new SunarpVehicleDto(
            result.Success, result.Brand, result.Model, result.Year,
            result.Color, result.Vin, result.EngineNumber, result.Titular, result.Error)));
    }

    private static ManagedVehicleDto ToDto(ManagedVehicle v) => new(
        v.MvId, v.Prcoid, v.LicensePlate, v.VehicleName,
        v.Brand, v.Model, v.Year, v.Color, v.Vin, v.EngineNumber,
        v.Source, v.Status, v.CreatedAt, v.UpdatedAt,
        null, null, false);
}

public sealed record ManagedVehicleDto(int MvId, int? Prcoid, string LicensePlate, string VehicleName,
    string? Brand, string? Model, short? Year, string? Color, string? Vin, string? EngineNumber,
    string Source, bool Status, DateTime CreatedAt, DateTime? UpdatedAt,
    int? CurrentKm, int? NextServiceKm, bool HasActiveOrder);

public sealed record CreateManagedVehicleRequest(string LicensePlate, string VehicleName, string? Brand,
    string? Model, short? Year, string? Color, string? Vin, string? EngineNumber);

public sealed record UpdateManagedVehicleRequest(string VehicleName, string? Brand,
    string? Model, short? Year, string? Color, string? Vin, string? EngineNumber);

public sealed record SunarpCaptchaDto(bool Success, string? CaptchaBase64, string? Error);
public sealed record SunarpVehicleDto(bool Success, string? Brand, string? Model, int? Year,
    string? Color, string? Vin, string? EngineNumber, string? Titular, string? Error);
public sealed record SunarpConsultRequest(string Plate, string CaptchaCode);
