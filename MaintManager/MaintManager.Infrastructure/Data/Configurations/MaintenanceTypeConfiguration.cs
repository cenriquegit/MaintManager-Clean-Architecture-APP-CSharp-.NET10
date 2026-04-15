// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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

