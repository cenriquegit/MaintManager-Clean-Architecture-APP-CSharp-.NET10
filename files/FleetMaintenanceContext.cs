// MaintManager.Infrastructure/Data/FleetMaintenanceContext.cs
using MaintManager.Domain.Entities;
using MaintManager.Domain.Entities.Existing;
using Microsoft.EntityFrameworkCore;

namespace MaintManager.Infrastructure.Data;

/// <summary>
/// DbContext único híbrido.
/// — Tablas existentes (list, public, product, company, service): configuradas como readonly.
/// — Tablas nuevas (maintenance.*): Code First con Fluent API completa.
/// </summary>
public sealed class FleetMaintenanceContext : DbContext
{
    public FleetMaintenanceContext(DbContextOptions<FleetMaintenanceContext> options)
        : base(options) { }

    // ── Tablas existentes (solo lectura) ────────────────────────────
    public DbSet<Vehicle>     Vehicles     { get; init; }
    public DbSet<Worker>      Workers      { get; init; }
    public DbSet<Person>      Persons      { get; init; }
    public DbSet<RentExecute> RentExecutes { get; init; }
    public DbSet<RentRequest> RentRequests { get; init; }

    // ── Tablas nuevas maintenance.* ──────────────────────────────────
    public DbSet<Maintenance>              Maintenances             { get; init; }
    public DbSet<MaintenanceType>          MaintenanceTypes         { get; init; }
    public DbSet<ServiceType>              ServiceTypes             { get; init; }
    public DbSet<ActionListType>           ActionListTypes          { get; init; }
    public DbSet<ActionCatalog>            ActionCatalogs           { get; init; }
    public DbSet<VehicleSchedule>          VehicleSchedules         { get; init; }
    public DbSet<ScheduleAction>           ScheduleActions          { get; init; }
    public DbSet<MaintenanceActionDetail>  ActionDetails            { get; init; }
    public DbSet<Diagnosis>                Diagnoses                { get; init; }
    public DbSet<MaterialCategory>         MaterialCategories       { get; init; }
    public DbSet<Material>                 Materials                { get; init; }
    public DbSet<MaterialLot>              MaterialLots             { get; init; }
    public DbSet<MaterialConsumption>      MaterialConsumptions     { get; init; }
    public DbSet<MaterialDiscard>          MaterialDiscards         { get; init; }
    public DbSet<MaterialRating>           MaterialRatings          { get; init; }
    public DbSet<InstalledComponent>       InstalledComponents      { get; init; }
    public DbSet<AlertConfig>              AlertConfigs             { get; init; }
    public DbSet<AlertLog>                 AlertLogs                { get; init; }
    public DbSet<ConfigSystem>             ConfigSystems            { get; init; }
    public DbSet<TechnicianAssignment>     TechnicianAssignments    { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones Fluent API
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetMaintenanceContext).Assembly);
    }
}
