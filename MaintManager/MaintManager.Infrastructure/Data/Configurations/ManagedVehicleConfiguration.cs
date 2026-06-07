using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class ManagedVehicleConfiguration : IEntityTypeConfiguration<ManagedVehicle>
{
    public void Configure(EntityTypeBuilder<ManagedVehicle> builder)
    {
        builder.ToTable("managed_vehicle", "maintenance");
        builder.HasKey(m => m.MvId);
        builder.Property(m => m.MvId).HasColumnName("mv_id").UseIdentityColumn();
        builder.Property(m => m.Prcoid).HasColumnName("prcoid");
        builder.Property(m => m.LicensePlate).HasColumnName("license_plate").HasMaxLength(20).IsRequired();
        builder.Property(m => m.VehicleName).HasColumnName("vehicle_name").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Brand).HasColumnName("brand").HasMaxLength(100);
        builder.Property(m => m.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(m => m.Year).HasColumnName("year");
        builder.Property(m => m.Color).HasColumnName("color").HasMaxLength(50);
        builder.Property(m => m.Vin).HasColumnName("vin").HasMaxLength(50);
        builder.Property(m => m.EngineNumber).HasColumnName("engine_number").HasMaxLength(50);
        builder.Property(m => m.Source).HasColumnName("source").HasMaxLength(20).HasDefaultValue("managed").IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasDefaultValue(true);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        builder.HasIndex(m => m.LicensePlate).IsUnique().HasDatabaseName("managed_vehicle_plate_unique");
    }
}
