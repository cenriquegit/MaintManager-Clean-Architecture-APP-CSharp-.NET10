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

        // Mapeo de la nueva propiedad
        builder.Property(vs => vs.NextServiceTypeCode)
               .HasColumnName("next_service_type_code")
               .HasMaxLength(1)
               .HasDefaultValue("A");

        builder.Property(vs => vs.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vs => vs.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(vs => vs.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vs => vs.Status).HasColumnName("status").HasDefaultValue(true);

        builder.HasIndex(vs => vs.Prcoid).IsUnique()
               .HasDatabaseName("vehicle_schedule_prcoid_unique");
    }
}