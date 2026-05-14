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

        builder.Property(m => m.Mainid).HasColumnName("mainid").UseIdentityColumn();
        builder.Property(m => m.Prcoid).HasColumnName("prcoid").IsRequired();
        builder.Property(m => m.Matyid).HasColumnName("matyid").IsRequired();
        builder.Property(m => m.Setyid).HasColumnName("setyid");
        builder.Property(m => m.OrderNumber).HasColumnName("order_number").HasMaxLength(30);
        builder.Property(m => m.MaintenanceDate).HasColumnName("maintenance_date")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.Mileage).HasColumnName("mileage").IsRequired();
        builder.Property(m => m.KmSinceLast).HasColumnName("km_since_last");
        builder.Property(m => m.AdditionalWork).HasColumnName("additional_work");
        builder.Property(m => m.OilBrand).HasColumnName("oil_brand").HasMaxLength(100);
        builder.Property(m => m.OilViscositySae).HasColumnName("oil_viscosity_sae").HasMaxLength(20);
        builder.Property(m => m.ClimateSeason).HasColumnName("climate_season").HasMaxLength(50);
        builder.Property(m => m.ShowOilInNextMaintenance).HasColumnName("show_oil_in_next_maintenance")
               .HasDefaultValue(false);
        builder.Property(m => m.OriginService).HasColumnName("origin_service").HasMaxLength(50)
               .HasDefaultValue("Taller propio");
        builder.Property(m => m.SignatureSeal).HasColumnName("signature_seal");
        builder.Property(m => m.IsEmergencyComplete).HasColumnName("is_emergency_complete");
        builder.Property(m => m.AssignedTo).HasColumnName("assigned_to");
        builder.Property(m => m.Workid).HasColumnName("workid").IsRequired();
        builder.Property(m => m.Note).HasColumnName("note");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(m => m.Statid).HasColumnName("statid").HasMaxLength(2)
               .HasDefaultValue("AC");

        // Relaciones
        builder.HasOne(m => m.MaintenanceType).WithMany().HasForeignKey(m => m.Matyid);
        builder.HasOne(m => m.ServiceType).WithMany().HasForeignKey(m => m.Setyid);
        builder.HasMany(m => m.ActionDetails).WithOne().HasForeignKey(d => d.Mainid)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Diagnosis).WithOne().HasForeignKey<Diagnosis>(d => d.Mainid)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.MaterialConsumptions).WithOne().HasForeignKey(mc => mc.Mainid)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.InstalledComponents).WithOne().HasForeignKey(ic => ic.Mainid)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.MaterialRatings).WithOne().HasForeignKey(mr => mr.Mainid)
               .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(m => m.Prcoid).HasDatabaseName("idx_maintenance_prcoid");
        builder.HasIndex(m => m.MaintenanceDate).HasDatabaseName("idx_maintenance_date");
        builder.HasIndex(m => m.Matyid).HasDatabaseName("idx_maintenance_matyid");
        builder.HasIndex(m => new { m.Prcoid, m.MaintenanceDate })
               .HasDatabaseName("idx_maintenance_prcoid_date");
    }
}