// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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

