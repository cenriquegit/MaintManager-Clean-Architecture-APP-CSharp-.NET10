using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class VehicleAllowedActionConfiguration : IEntityTypeConfiguration<VehicleAllowedAction>
{
    public void Configure(EntityTypeBuilder<VehicleAllowedAction> builder)
    {
        builder.ToTable("vehicle_allowed_action", "maintenance");
        builder.HasKey(v => v.Vaacid);
        builder.Property(v => v.Vaacid).HasColumnName("vaacid").UseIdentityColumn();
        builder.Property(v => v.Prcoid).HasColumnName("prcoid").IsRequired(false);
        builder.Property(v => v.MvId).HasColumnName("mv_id").IsRequired(false);
        builder.Property(v => v.Acatid).HasColumnName("acatid").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.HasIndex(v => new { v.Prcoid, v.Acatid }).IsUnique().HasDatabaseName("uq_vehicle_allowed_action");
    }
}
