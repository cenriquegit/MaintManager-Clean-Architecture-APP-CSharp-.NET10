// MaintManager.API/Controllers/AuthController.cs
using FluentValidation;
using MaintManager.Application.DTOs.Auth;
using MaintManager.Application.DTOs.Common;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MaintManager.API.Controllers;

/// <summary>Autenticación de usuarios del sistema.</summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;
    private readonly IConfiguration _configuration;
    private readonly IValidator<LoginRequest> _validator;

    public AuthController(
        FleetMaintenanceContext context,
        IConfiguration configuration,
        IValidator<LoginRequest> validator)
    {
        _context = context;
        _configuration = configuration;
        _validator = validator;
    }

    /// <summary>Iniciar sesión con usuario y contraseña.</summary>
    /// <response code="200">Token JWT generado correctamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    /// <response code="401">Credenciales incorrectas o usuario bloqueado.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var passwordHash = ComputeMd5(request.Password);

        var worker = await _context.Workers
            .Include(w => w.Person)
            .FirstOrDefaultAsync(w =>
                w.Username == request.Username &&
                w.Password == passwordHash &&
                w.Status, ct);

        if (worker is null)
            return Unauthorized(ApiResponse<object>.Fail(ErrorMessages.Auth.InvalidCredentials));

        if (worker.Locked)
            return Unauthorized(ApiResponse<object>.Fail(ErrorMessages.Auth.UserLocked));

        // Determinar rol según jobid
        // jobid de "Mecánico Técnico" → Tecnico; resto → Admin
        var role = await DetermineRoleAsync(worker.Jobid, ct);
        var (token, expiresAt) = GenerateJwt(worker.Workid, worker.Username!, role,
            worker.Person?.Name ?? string.Empty);

        var response = new LoginResponse(
            Token: token,
            Username: worker.Username!,
            FullName: $"{worker.Person?.Fln} {worker.Person?.Mln} {worker.Person?.Name}".Trim(),
            Role: role,
            ExpiresAt: expiresAt
        );

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    private async Task<string> DetermineRoleAsync(short jobid, CancellationToken ct)
    {
        var job = await _context.Set<MaintManager.Domain.Entities.Existing.Worker>()
            .AsNoTracking()
            .Select(w => new { w.Jobid })
            .FirstOrDefaultAsync(ct);

        // Si el cargo tiene "Mecánico" en el nombre → Tecnico; sino → Admin
        // Consultamos el nombre del job directamente
        var jobName = await _context.Database
            .SqlQueryRaw<string>(
                "SELECT name FROM public.job WHERE jobid = {0}", jobid)
            .FirstOrDefaultAsync(ct);

        return jobName?.Contains("Mecánico", StringComparison.OrdinalIgnoreCase) == true
            ? RoleNames.Tecnico
            : RoleNames.Admin;
    }

    private (string token, DateTime expiresAt) GenerateJwt(
        int workid, string username, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8");
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, workid.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.GivenName, fullName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static string ComputeMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.API/Controllers/VehiclesController.cs
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Vehicle;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.API.Controllers;

/// <summary>Consulta de vehículos de la flota (solo lectura desde BD existente).</summary>
[ApiController]
[Route("api/v1/vehicles")]
[Authorize]
[Produces("application/json")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ISchedulingService _schedulingService;
    private readonly FleetMaintenanceContext _context;

    public VehiclesController(
        IVehicleRepository vehicleRepo,
        ISchedulingService schedulingService,
        FleetMaintenanceContext context)
    {
        _vehicleRepo = vehicleRepo;
        _schedulingService = schedulingService;
        _context = context;
    }

    /// <summary>Obtener lista de todos los vehículos activos de la flota.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VehicleListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var vehicles = await _vehicleRepo.GetActiveVehiclesAsync(ct);
        var result = new List<VehicleListItem>();

        foreach (var v in vehicles)
        {
            var currentKm = await _vehicleRepo.GetCurrentKmAsync(v.Prcoid, ct);
            var schedule = await _schedulingService.GetScheduleAsync(v.Prcoid, ct);
            var isDueSoon = await _schedulingService.IsMaintenanceDueSoonAsync(v.Prcoid, ct);
            result.Add(v.ToListItem(currentKm, schedule?.NextKm, isDueSoon));
        }

        return Ok(ApiResponse<IReadOnlyList<VehicleListItem>>.Ok(result));
    }

    /// <summary>Obtener detalle de un vehículo por su ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id, ct);
        if (vehicle is null)
            return NotFound(ApiResponse<object>.Fail(ErrorMessages.Vehicle.NotFound));

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        var schedule = await _schedulingService.GetScheduleAsync(id, ct);
        var isDueSoon = await _schedulingService.IsMaintenanceDueSoonAsync(id, ct);

        return Ok(ApiResponse<VehicleResponse>.Ok(
            vehicle.ToResponse(currentKm, schedule?.NextKm, schedule?.AlertKmThreshold, isDueSoon)));
    }

    /// <summary>Obtener el km actual de un vehículo.</summary>
    [HttpGet("{id:int}/current-km")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentKm(int id, CancellationToken ct)
    {
        var km = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        return Ok(ApiResponse<int>.Ok(km));
    }

    /// <summary>Obtener el cronograma de mantenimiento de un vehículo.</summary>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedule(int id, CancellationToken ct)
    {
        var schedule = await _schedulingService.GetScheduleAsync(id, ct);
        if (schedule is null)
            return NotFound(ApiResponse<object>.Fail("Este vehículo no tiene cronograma configurado."));

        var currentKm = await _vehicleRepo.GetCurrentKmAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            schedule.Veshid,
            schedule.Prcoid,
            schedule.IntervalKm,
            schedule.NextKm,
            schedule.AlertKmThreshold,
            CurrentKm = currentKm,
            KmUntilService = schedule.NextKm - currentKm
        }));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.API/Controllers/MaintenancesController.cs
using FluentValidation;
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Maintenance;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MaintManager.API.Controllers;

/// <summary>Gestión de órdenes de mantenimiento vehicular.</summary>
[ApiController]
[Route("api/v1/maintenances")]
[Authorize]
[Produces("application/json")]
public sealed class MaintenancesController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly FleetMaintenanceContext _context;
    private readonly IValidator<MaintenanceCreateRequest> _createValidator;

    public MaintenancesController(
        IMaintenanceService maintenanceService,
        IMaintenanceRepository maintenanceRepo,
        IVehicleRepository vehicleRepo,
        FleetMaintenanceContext context,
        IValidator<MaintenanceCreateRequest> createValidator)
    {
        _maintenanceService = maintenanceService;
        _maintenanceRepo = maintenanceRepo;
        _vehicleRepo = vehicleRepo;
        _context = context;
        _createValidator = createValidator;
    }

    /// <summary>Obtener lista paginada de mantenimientos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MaintenanceListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest paged, CancellationToken ct)
    {
        var items = await _maintenanceRepo.GetPagedAsync(paged.Page, paged.PageSize, ct);
        var result = new List<MaintenanceListItem>();

        foreach (var m in items)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(m.Prcoid, ct);
            var assignedWorker = await _context.Workers
                .Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == m.AssignedTo, ct);
            var assignedName = assignedWorker?.Person?.Name ?? "Sin asignar";

            result.Add(m.ToListItem(
                vehicle?.LicensePlateNumber ?? string.Empty,
                vehicle?.Product?.Name ?? string.Empty,
                assignedName));
        }

        var totalCount = await _context.Maintenances.CountAsync(m => m.Statid == "AC", ct);
        var response = new PagedResponse<MaintenanceListItem>
        {
            Items = result,
            TotalCount = totalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

        return Ok(ApiResponse<PagedResponse<MaintenanceListItem>>.Ok(response));
    }

    /// <summary>Obtener detalle de una orden de mantenimiento.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        try
        {
            var maintenance = await _maintenanceService.GetWithDetailsAsync(id, ct);
            var vehicle = await _vehicleRepo.GetByIdAsync(maintenance.Prcoid, ct);

            var assignedWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.AssignedTo, ct);
            var registeredWorker = await _context.Workers.Include(w => w.Person)
                .FirstOrDefaultAsync(w => w.Workid == maintenance.Workid, ct);

            return Ok(ApiResponse<MaintenanceResponse>.Ok(maintenance.ToResponse(
                vehicle?.LicensePlateNumber ?? string.Empty,
                vehicle?.Product?.Name ?? string.Empty,
                assignedWorker?.Person?.Name ?? string.Empty,
                registeredWorker?.Person?.Name ?? string.Empty)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Crear una nueva orden de mantenimiento.</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] MaintenanceCreateRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var registeredBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var maintenance = await _maintenanceService.CreateAsync(
                request.Prcoid, request.Matyid, request.Mileage,
                request.AssignedTo, registeredBy, request.Setyid, request.Note, ct);

            return CreatedAtAction(nameof(GetById),
                new { id = maintenance.Mainid },
                ApiResponse<int>.Ok(maintenance.Mainid));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Completar una acción del checklist de la orden.</summary>
    [HttpPut("{id:int}/actions/{actionId:int}/complete")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteAction(
        int id, int actionId,
        [FromBody] ActionCompleteRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.CompleteActionAsync(
                actionId, request.ActionCode, request.ProductUsed,
                request.QuantityUsed, request.OriginProduct, request.Observation,
                request.Maloid, ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Acción completada correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Guardar el diagnóstico final del mecánico.</summary>
    [HttpPost("{id:int}/diagnosis")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveDiagnosis(
        int id,
        [FromBody] DiagnosisRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.SaveDiagnosisAsync(
                id, request.GeneralStatus, request.VehicleOperative,
                request.Observations, request.FutureRecommendations, ct);

            return Ok(ApiResponse<object>.Ok(new { message = "Diagnóstico guardado correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Cerrar una orden de mantenimiento.</summary>
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Tecnico}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Close(
        int id,
        [FromBody] MaintenanceCloseRequest request,
        CancellationToken ct)
    {
        try
        {
            await _maintenanceService.CloseMaintenanceAsync(id, request.IsEmergencyComplete, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Orden cerrada correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Obtener todos los mantenimientos de un vehículo.</summary>
    [HttpGet("~/api/v1/vehicles/{vehicleId:int}/maintenances")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaintenanceListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVehicle(int vehicleId, CancellationToken ct)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId, ct);
        var maintenances = await _maintenanceService.GetByVehicleAsync(vehicleId, ct);
        var result = maintenances.Select(m => m.ToListItem(
            vehicle?.LicensePlateNumber ?? string.Empty,
            vehicle?.Product?.Name ?? string.Empty,
            string.Empty)).ToList();

        return Ok(ApiResponse<IReadOnlyList<MaintenanceListItem>>.Ok(result));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.API/Controllers/InventoryController.cs
using FluentValidation;
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Inventory;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaintManager.API.Controllers;

/// <summary>Gestión del inventario de materiales y lotes.</summary>
[ApiController]
[Route("api/v1/inventory")]
[Authorize]
[Produces("application/json")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IValidator<MaterialCreateRequest> _materialValidator;
    private readonly IValidator<LotCreateRequest> _lotValidator;

    public InventoryController(
        IInventoryService inventoryService,
        IInventoryRepository inventoryRepo,
        IValidator<MaterialCreateRequest> materialValidator,
        IValidator<LotCreateRequest> lotValidator)
    {
        _inventoryService = inventoryService;
        _inventoryRepo = inventoryRepo;
        _materialValidator = materialValidator;
        _lotValidator = lotValidator;
    }

    /// <summary>Obtener todos los materiales del inventario.</summary>
    [HttpGet("materials")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaterialListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterials(CancellationToken ct)
    {
        var materials = await _inventoryRepo.GetMaterialsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<MaterialListItem>>.Ok(
            materials.Select(m => m.ToListItem()).ToList()));
    }

    /// <summary>Obtener detalle de un material con sus lotes activos.</summary>
    [HttpGet("materials/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MaterialResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterialById(int id, CancellationToken ct)
    {
        var material = await _inventoryRepo.GetMaterialByIdAsync(id, ct);
        if (material is null)
            return NotFound(ApiResponse<object>.Fail(ErrorMessages.Inventory.MaterialNotFound));

        return Ok(ApiResponse<MaterialResponse>.Ok(material.ToResponse()));
    }

    /// <summary>Crear un nuevo material en inventario. Solo Admin.</summary>
    [HttpPost("materials")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMaterial(
        [FromBody] MaterialCreateRequest request, CancellationToken ct)
    {
        var validation = await _materialValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var createdBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _inventoryService.CreateMaterialAsync(
            request.Macaid, request.Name, request.UnitOfMeasure,
            request.StockMinimum, createdBy, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<object>.Ok(new { message = "Material creado correctamente." }));
    }

    /// <summary>Ingresar un nuevo lote de material.</summary>
    [HttpPost("materials/{id:int}/lots")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterLot(
        int id, [FromBody] LotCreateRequest request, CancellationToken ct)
    {
        var validation = await _lotValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var createdBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _inventoryService.RegisterLotAsync(
            id, request.Quantity, request.UnitCost, createdBy,
            request.ExpirationDate, request.Provid, request.SupplierLotNumber, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<object>.Ok(new { message = "Lote ingresado correctamente." }));
    }

    /// <summary>Descartar cantidad de un lote. Solo Admin.</summary>
    [HttpPost("lots/{lotId:int}/discard")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DiscardLot(
        int lotId, [FromBody] LotDiscardRequest request, CancellationToken ct)
    {
        var discardedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _inventoryService.DiscardLotAsync(
                lotId, request.Quantity, request.Reason, discardedBy, request.Note, ct);
            return Ok(ApiResponse<object>.Ok(new { message = "Descarte registrado correctamente." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Materiales con stock por debajo del mínimo.</summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MaterialListItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        var materials = await _inventoryRepo.GetLowStockMaterialsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<MaterialListItem>>.Ok(
            materials.Select(m => m.ToListItem()).ToList()));
    }

    /// <summary>Lotes próximos a vencer (próximos 30 días por defecto).</summary>
    [HttpGet("expiring-lots")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LotResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringLots(
        [FromQuery] int days = 30, CancellationToken ct = default)
    {
        var lots = await _inventoryRepo.GetExpiringLotsAsync(days, ct);
        return Ok(ApiResponse<IReadOnlyList<LotResponse>>.Ok(
            lots.Select(l => l.ToResponse()).ToList()));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.API/Controllers/AlertsController.cs
using MaintManager.Application.DTOs.Common;
using MaintManager.Application.DTOs.Reports;
using MaintManager.Application.Mappings;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaintManager.API.Controllers;

/// <summary>Gestión del sistema de alertas.</summary>
[ApiController]
[Route("api/v1/alerts")]
[Authorize]
[Produces("application/json")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepo;
    private readonly IAlertService _alertService;

    public AlertsController(IAlertRepository alertRepo, IAlertService alertService)
    {
        _alertRepo = alertRepo;
        _alertService = alertService;
    }

    /// <summary>Obtener alertas no resueltas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnresolved(CancellationToken ct)
    {
        var alerts = await _alertRepo.GetUnresolvedAlertsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<AlertResponse>>.Ok(
            alerts.Select(a => a.ToAlertResponse()).ToList()));
    }

    /// <summary>Marcar alerta como leída.</summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var workid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _alertService.MarkAsReadAsync(id, workid, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Alerta marcada como leída." }));
    }

    /// <summary>Resolver una alerta. Solo Admin.</summary>
    [HttpPut("{id:int}/resolve")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(int id, CancellationToken ct)
    {
        var workid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _alertService.ResolveAsync(id, workid, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Alerta resuelta correctamente." }));
    }

    /// <summary>Ejecutar verificación de alertas manualmente.</summary>
    [HttpPost("check")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAlerts(CancellationToken ct)
    {
        await _alertService.CheckAndGenerateAlertsAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Verificación completada." }));
    }
}
