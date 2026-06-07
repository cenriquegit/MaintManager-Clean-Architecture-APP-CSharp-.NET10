using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class VehicleAllowedComponentConfiguration : IEntityTypeConfiguration<VehicleAllowedComponent>
{
    public void Configure(EntityTypeBuilder<VehicleAllowedComponent> builder)
    {
        builder.ToTable("vehicle_allowed_component", "maintenance");
        builder.HasKey(v => v.Vacoid);
        builder.Property(v => v.Vacoid).HasColumnName("vacoid").UseIdentityColumn();
        builder.Property(v => v.Prcoid).HasColumnName("prcoid").IsRequired(false);
        builder.Property(v => v.MvId).HasColumnName("mv_id").IsRequired(false);
        builder.Property(v => v.Acatid).HasColumnName("acatid").IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.HasIndex(v => new { v.Prcoid, v.Acatid }).IsUnique().HasDatabaseName("uq_vehicle_allowed_component");
    }
}
