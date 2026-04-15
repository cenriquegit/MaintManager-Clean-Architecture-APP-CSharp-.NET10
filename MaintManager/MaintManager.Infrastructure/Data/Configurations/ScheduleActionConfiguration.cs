// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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
        builder.HasOne(sa => sa.VehicleSchedule).WithMany().HasForeignKey(sa => sa.Veshid);
        builder.HasOne(sa => sa.ActionCatalog).WithMany().HasForeignKey(sa => sa.Acatid);
    }
}

