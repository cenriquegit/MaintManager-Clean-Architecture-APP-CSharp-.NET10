// MaintManager.API/Program.cs
using FluentValidation;
using MaintManager.Application.Services;
using MaintManager.Application.Validators;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Data;
using MaintManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/maintmanager-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Base de datos ─────────────────────────────────────────────────
builder.Services.AddDbContext<FleetMaintenanceContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("MaintManager.Infrastructure"))
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .LogTo(Log.Logger.Information, LogLevel.Warning));

// ── JWT ────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key no configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Repositorios ──────────────────────────────────────────────────
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// ── Servicios de aplicación ──────────────────────────────────────
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAlertService, AlertService>();

// ── Validaciones ─────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ── Controllers ───────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MaintManager API",
        Version = "v1",
        Description = "Sistema de Gestión de Mantenimiento Vehicular con BI — Neo Plus Business S.A.C.",
        Contact = new OpenApiContact
        {
            Name = "Carlos Tarazona",
            Email = "1560977@senati.pe"
        }
    });

    // Soporte JWT en Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingresar 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // XML comments para documentación
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// ── CORS (para MAUI en Windows) ──────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("MauiPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MaintManager API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("MauiPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.API/Middleware/GlobalExceptionMiddleware.cs
namespace MaintManager.API;

/// <summary>
/// Intercepta todas las excepciones no manejadas y devuelve una respuesta
/// estandarizada en formato JSON. Nunca expone detalles técnicos al cliente.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso no encontrado: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación inválida: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden,
                "No tienes permiso para realizar esta acción.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en {Path}", context.Request.Path);
            await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Por favor intente nuevamente.");
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { success = false, message });
    }
}
