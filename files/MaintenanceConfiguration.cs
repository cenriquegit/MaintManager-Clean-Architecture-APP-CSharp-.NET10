// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        builder.ToTable("maintenance", "maintenance");
        builder.HasKey(m => m.Mainid);

        builder.Property(m => m.Mainid).HasColumnName("mainid")
               .UseIdentityColumn();
        builder.Property(m => m.Prcoid).HasColumnName("prcoid").IsRequired();
        builder.Property(m => m.Matyid).HasColumnName("matyid").IsRequired();
        builder.Property(m => m.Setyid).HasColumnName("setyid");
        builder.Property(m => m.OrderNumber).HasColumnName("order_number").HasMaxLength(30);
        builder.Property(m => m.MaintenanceDate).HasColumnName("maintenance_date").IsRequired();
        builder.Property(m => m.Mileage).HasColumnName("mileage").IsRequired();
        builder.Property(m => m.KmSinceLast).HasColumnName("km_since_last");
        builder.Property(m => m.AdditionalWork).HasColumnName("additional_work");
        builder.Property(m => m.OilBrand).HasColumnName("oil_brand").HasMaxLength(100);
        builder.Property(m => m.OilViscositySae).HasColumnName("oil_viscosity_sae").HasMaxLength(20);
        builder.Property(m => m.ClimateSeason).HasColumnName("climate_season").HasMaxLength(50);
        builder.Property(m => m.ShowOilInNextMaintenance)
               .HasColumnName("show_oil_in_next_maintenance").HasDefaultValue(false);
        builder.Property(m => m.OriginService).HasColumnName("origin_service")
               .HasMaxLength(50).HasDefaultValue("Taller propio");
        builder.Property(m => m.SignatureSeal).HasColumnName("signature_seal");
        builder.Property(m => m.IsEmergencyComplete).HasColumnName("is_emergency_complete");
        builder.Property(m => m.AssignedTo).HasColumnName("assigned_to").IsRequired();
        builder.Property(m => m.Workid).HasColumnName("workid").IsRequired();
        builder.Property(m => m.Note).HasColumnName("note");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.Statid).HasColumnName("statid").HasMaxLength(2)
               .HasDefaultValue("AC");

        builder.HasOne(m => m.MaintenanceType)
               .WithMany().HasForeignKey(m => m.Matyid);
        builder.HasOne(m => m.ServiceType)
               .WithMany().HasForeignKey(m => m.Setyid);
        builder.HasMany(m => m.ActionDetails)
               .WithOne().HasForeignKey(d => d.Mainid).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Diagnosis)
               .WithOne().HasForeignKey<Diagnosis>(d => d.Mainid).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.MaterialConsumptions)
               .WithOne().HasForeignKey(c => c.Mainid).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.InstalledComponents)
               .WithOne().HasForeignKey(ic => ic.Mainid).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.MaterialRatings)
               .WithOne().HasForeignKey(r => r.Mainid).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Prcoid).HasDatabaseName("idx_maintenance_prcoid");
        builder.HasIndex(m => m.MaintenanceDate).HasDatabaseName("idx_maintenance_date");
        builder.HasIndex(m => m.Matyid).HasDatabaseName("idx_maintenance_matyid");
        builder.HasIndex(m => new { m.Prcoid, m.MaintenanceDate })
               .HasDatabaseName("idx_maintenance_prcoid_date");
        builder.HasIndex(m => new { m.Statid, m.Matyid })
               .HasDatabaseName("idx_maintenance_statid_matyid");
        builder.HasIndex(m => new { m.AssignedTo, m.MaintenanceDate })
               .HasDatabaseName("idx_maintenance_assigned_date");
    }
}

internal sealed class MaintenanceTypeConfiguration : IEntityTypeConfiguration<MaintenanceType>
{
    public void Configure(EntityTypeBuilder<MaintenanceType> builder)
    {
        builder.ToTable("maintenance_type", "maintenance");
        builder.HasKey(mt => mt.Matyid);
        builder.Property(mt => mt.Matyid).HasColumnName("matyid").UseIdentityColumn();
        builder.Property(mt => mt.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(mt => mt.Description).HasColumnName("description");
        builder.Property(mt => mt.Status).HasColumnName("status").HasDefaultValue(true);
        builder.HasIndex(mt => mt.Name).IsUnique().HasDatabaseName("maintenance_type_name_unique");
    }
}

internal sealed class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.ToTable("service_type", "maintenance");
        builder.HasKey(st => st.Setyid);
        builder.Property(st => st.Setyid).HasColumnName("setyid").UseIdentityColumn();
        builder.Property(st => st.Code).HasColumnName("code").HasMaxLength(1).IsRequired();
        builder.Property(st => st.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(st => st.Description).HasColumnName("description");
        builder.Property(st => st.Status).HasColumnName("status").HasDefaultValue(true);
    }
}

internal sealed class ActionListTypeConfiguration : IEntityTypeConfiguration<ActionListType>
{
    public void Configure(EntityTypeBuilder<ActionListType> builder)
    {
        builder.ToTable("action_list_type", "maintenance");
        builder.HasKey(a => a.Altoid);
        builder.Property(a => a.Altoid).HasColumnName("altoid").UseIdentityColumn();
        builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Status).HasColumnName("status").HasDefaultValue(true);
    }
}

internal sealed class ActionCatalogConfiguration : IEntityTypeConfiguration<ActionCatalog>
{
    public void Configure(EntityTypeBuilder<ActionCatalog> builder)
    {
        builder.ToTable("action_catalog", "maintenance");
        builder.HasKey(a => a.Acatid);
        builder.Property(a => a.Acatid).HasColumnName("acatid").UseIdentityColumn();
        builder.Property(a => a.Altoid).HasColumnName("altoid").IsRequired();
        builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(a => a.Category).HasColumnName("category").HasMaxLength(80);
        builder.Property(a => a.RecommendedProduct).HasColumnName("recommended_product").HasMaxLength(200);
        builder.Property(a => a.RecommendedQuantity).HasColumnName("recommended_quantity").HasMaxLength(50);
        builder.Property(a => a.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(30);
        builder.Property(a => a.UsefulLifeKm).HasColumnName("useful_life_km");
        builder.Property(a => a.ExpiresByTime).HasColumnName("expires_by_time").HasDefaultValue(false);
        builder.Property(a => a.UsefulLifeDays).HasColumnName("useful_life_days");
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Status).HasColumnName("status").HasDefaultValue(true);

        builder.HasOne(a => a.ActionListType)
               .WithMany(alt => alt.Actions).HasForeignKey(a => a.Altoid);
    }
}

internal sealed class VehicleScheduleConfiguration : IEntityTypeConfiguration<VehicleSchedule>
{
    public void Configure(EntityTypeBuilder<VehicleSchedule> builder)
    {
        builder.ToTable("vehicle_schedule", "maintenance");
        builder.HasKey(vs => vs.Veshid);
        builder.Property(vs => vs.Veshid).HasColumnName("veshid").UseIdentityColumn();
        builder.Property(vs => vs.Prcoid).HasColumnName("prcoid").IsRequired();
        builder.Property(vs => vs.IntervalKm).HasColumnName("interval_km").HasDefaultValue(5000);
        builder.Property(vs => vs.NextKm).HasColumnName("next_km").IsRequired();
        builder.Property(vs => vs.AlertKmThreshold).HasColumnName("alert_km_threshold").HasDefaultValue(800);
        builder.Property(vs => vs.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vs => vs.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(vs => vs.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vs => vs.Status).HasColumnName("status").HasDefaultValue(true);

        // [AJUSTE 3] next_service_type_code
        builder.Property<string>("NextServiceTypeCode")
               .HasColumnName("next_service_type_code").HasMaxLength(1).HasDefaultValue("A");

        builder.HasIndex(vs => vs.Prcoid).IsUnique()
               .HasDatabaseName("vehicle_schedule_prcoid_unique");
    }
}

internal sealed class ScheduleActionConfiguration : IEntityTypeConfiguration<ScheduleAction>
{
    public void Configure(EntityTypeBuilder<ScheduleAction> builder)
    {
        builder.ToTable("schedule_action", "maintenance");
        builder.HasKey(sa => sa.Shacid);
        builder.Property(sa => sa.Shacid).HasColumnName("shacid").UseIdentityColumn();
        builder.Property(sa => sa.Veshid).HasColumnName("veshid").IsRequired();
        builder.Property(sa => sa.Acatid).HasColumnName("acatid").IsRequired();
        builder.Property(sa => sa.ScheduledKm).HasColumnName("scheduled_km").IsRequired();
        builder.Property(sa => sa.ActionCode).HasColumnName("action_code").HasMaxLength(1).IsRequired();
        builder.Property(sa => sa.Status).HasColumnName("status").HasDefaultValue(true);

        builder.HasOne(sa => sa.VehicleSchedule)
               .WithMany().HasForeignKey(sa => sa.Veshid);
        builder.HasOne(sa => sa.ActionCatalog)
               .WithMany().HasForeignKey(sa => sa.Acatid);
    }
}

internal sealed class MaintenanceActionDetailConfiguration : IEntityTypeConfiguration<MaintenanceActionDetail>
{
    public void Configure(EntityTypeBuilder<MaintenanceActionDetail> builder)
    {
        builder.ToTable("maintenance_action_detail", "maintenance");
        builder.HasKey(d => d.Madeid);
        builder.Property(d => d.Madeid).HasColumnName("madeid").UseIdentityColumn();
        builder.Property(d => d.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(d => d.Acatid).HasColumnName("acatid").IsRequired();
        builder.Property(d => d.Completed).HasColumnName("completed").HasDefaultValue(false);
        builder.Property(d => d.ActionPerformed).HasColumnName("action_performed").HasMaxLength(1);
        builder.Property(d => d.ProductUsed).HasColumnName("product_used").HasMaxLength(200);
        builder.Property(d => d.QuantityUsed).HasColumnName("quantity_used").HasMaxLength(50);
        builder.Property(d => d.OriginProduct).HasColumnName("origin_product").HasMaxLength(50);
        builder.Property(d => d.Observation).HasColumnName("observation");
        builder.Property(d => d.Maloid).HasColumnName("maloid");

        builder.HasOne(d => d.ActionCatalog)
               .WithMany().HasForeignKey(d => d.Acatid);
        builder.HasOne(d => d.MaterialLot)
               .WithMany().HasForeignKey(d => d.Maloid);
    }
}

internal sealed class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("diagnosis", "maintenance");
        builder.HasKey(d => d.Diagid);
        builder.Property(d => d.Diagid).HasColumnName("diagid").UseIdentityColumn();
        builder.Property(d => d.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(d => d.GeneralStatus).HasColumnName("general_status").HasMaxLength(100).IsRequired();
        builder.Property(d => d.Observations).HasColumnName("observations");
        builder.Property(d => d.VehicleOperative).HasColumnName("vehicle_operative").HasDefaultValue(true);
        builder.Property(d => d.FutureRecommendations).HasColumnName("future_recommendations");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(d => d.Mainid).IsUnique().HasDatabaseName("diagnosis_mainid_unique");
    }
}
