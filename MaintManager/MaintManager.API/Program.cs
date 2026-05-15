using FluentValidation;
using MaintManager.API.Middleware;
using QuestPDF;
using QuestPDF.Infrastructure;
using MaintManager.Application.Services;
using MaintManager.Application.Validators;
using MaintManager.Domain.Interfaces.Services;
using MaintManager.Infrastructure.Services;
using MaintManager.Domain.Interfaces.Repositories;
using MaintManager.Infrastructure.Data;
using MaintManager.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

Console.WriteLine(">>> Program.cs iniciado (línea 1)");

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(">>> WebApplication.CreateBuilder completado");

// Usar logging simple de .NET para ver en consola
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

Console.WriteLine(">>> Logging configurado");

// ── Base de datos ─────────────────────────────────────────────────
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($">>> Cadena de conexión: {connString}");

builder.Services.AddDbContext<FleetMaintenanceContext>(options =>
{
    options.UseNpgsql(connString,
        npgsql => npgsql.MigrationsAssembly("MaintManager.Infrastructure"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});
Console.WriteLine(">>> DbContext registrado");

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
Console.WriteLine(">>> JWT configurado");

// ── Repositorios ──────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
Console.WriteLine(">>> Repositorios registrados");

// ── Servicios de aplicación ──────────────────────────────────────
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IBiReportService, BiReportService>();
Console.WriteLine(">>> Servicios registrados");

// ── Validaciones ─────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
Console.WriteLine(">>> Validadores registrados");

// ── Controllers ───────────────────────────────────────────────────
builder.Services.AddControllers();
Console.WriteLine(">>> Controllers registrados");

// ── Swagger ───────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MaintManager API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
});
Console.WriteLine(">>> Swagger registrado");

// ── CORS ───────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("MauiPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
Console.WriteLine(">>> CORS registrado");

QuestPDF.Settings.License = LicenseType.Community;
Console.WriteLine(">>> QuestPDF license configurada");

Console.WriteLine(">>> Construyendo la aplicación (builder.Build())...");
var app = builder.Build();
Console.WriteLine(">>> Aplicación construida.");

// ── Middleware pipeline ───────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MaintManager API v1");
        c.RoutePrefix = string.Empty;
    });
    Console.WriteLine(">>> Swagger UI configurado");
}

app.UseMiddleware<GlobalExceptionMiddleware>();
Console.WriteLine(">>> Middleware de excepción agregado");

app.UseCors("MauiPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
Console.WriteLine(">>> Middleware y endpoints mapeados");

Console.WriteLine($">>> Iniciando app.Run() en: {string.Join(", ", app.Urls)}");
app.Run();