// MaintManager.Infrastructure/Data/Configurations/MaintenanceConfiguration.cs
// ACTUALIZADO: maintenance.maintenance NO tiene assigned_to en la BD base.
// assigned_to se agrega vía 02_ajustes_fase1.sql [A1].
// La entidad Maintenance.cs SÍ tiene la propiedad (ya existe por el ajuste).
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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
