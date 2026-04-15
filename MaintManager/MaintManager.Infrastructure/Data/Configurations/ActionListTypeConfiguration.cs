// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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

