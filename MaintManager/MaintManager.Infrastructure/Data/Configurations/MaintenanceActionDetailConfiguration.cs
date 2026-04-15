// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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
        builder.HasOne(d => d.ActionCatalog).WithMany().HasForeignKey(d => d.Acatid);
        builder.HasOne(d => d.MaterialLot).WithMany().HasForeignKey(d => d.Maloid);
    }
}

