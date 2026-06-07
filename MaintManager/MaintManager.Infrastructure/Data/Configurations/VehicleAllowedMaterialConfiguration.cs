using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class VehicleAllowedMaterialConfiguration : IEntityTypeConfiguration<VehicleAllowedMaterial>
{
    public void Configure(EntityTypeBuilder<VehicleAllowedMaterial> builder)
    {
        builder.ToTable("vehicle_allowed_material", "maintenance");
        builder.HasKey(v => v.Vamid);
        builder.Property(v => v.Vamid).HasColumnName("vamid").UseIdentityColumn();
        builder.Property(v => v.Prcoid).HasColumnName("prcoid").IsRequired(false);
        builder.Property(v => v.MvId).HasColumnName("mv_id").IsRequired(false);
        builder.Property(v => v.Mateid).HasColumnName("mateid").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.HasIndex(v => new { v.Prcoid, v.Mateid }).IsUnique().HasDatabaseName("uq_vehicle_allowed_material");
    }
}
