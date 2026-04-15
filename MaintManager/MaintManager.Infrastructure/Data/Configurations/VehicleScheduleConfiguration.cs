// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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

        // next_service_type_code: columna agregada por 02_ajustes_fase1.sql [A3]
        builder.Property<string>("NextServiceTypeCode")
               .HasColumnName("next_service_type_code").HasMaxLength(1).HasDefaultValue("A");

        builder.HasIndex(vs => vs.Prcoid).IsUnique()
               .HasDatabaseName("vehicle_schedule_prcoid_unique");
    }
}

