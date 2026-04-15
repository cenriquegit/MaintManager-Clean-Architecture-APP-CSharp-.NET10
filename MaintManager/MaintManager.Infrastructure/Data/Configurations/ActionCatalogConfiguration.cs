// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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
        builder.HasOne(a => a.ActionListType).WithMany(alt => alt.Actions).HasForeignKey(a => a.Altoid);
    }
}

