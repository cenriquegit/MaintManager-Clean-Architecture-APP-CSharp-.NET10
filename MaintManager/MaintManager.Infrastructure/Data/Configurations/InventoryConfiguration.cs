// MaintManager.Infrastructure/Data/Configurations/InventoryConfiguration.cs
// ACTUALIZADO:
// — MaterialConfiguration: updated_at se agrega por 02_ajustes_fase1.sql [A2]
// — TechnicianAssignmentConfiguration: tabla agregada por 02_ajustes_fase1.sql [A4]
using MaintManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

internal sealed class MaterialCategoryConfiguration : IEntityTypeConfiguration<MaterialCategory>
{
    public void Configure(EntityTypeBuilder<MaterialCategory> builder)
    {
        builder.ToTable("material_category", "maintenance");
        builder.HasKey(mc => mc.Macaid);
        builder.Property(mc => mc.Macaid).HasColumnName("macaid").UseIdentityColumn();
        builder.Property(mc => mc.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(mc => mc.Description).HasColumnName("description");
        builder.Property(mc => mc.Status).HasColumnName("status").HasDefaultValue(true);
        builder.HasIndex(mc => mc.Name).IsUnique().HasDatabaseName("material_category_name_unique");
    }
}

internal sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("material", "maintenance");
        builder.HasKey(m => m.Mateid);
        builder.Property(m => m.Mateid).HasColumnName("mateid").UseIdentityColumn();
        builder.Property(m => m.Macaid).HasColumnName("macaid").IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(m => m.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(30).IsRequired();
        builder.Property(m => m.StockTotal).HasColumnName("stock_total").HasPrecision(12, 3).HasDefaultValue(0);
        builder.Property(m => m.StockMinimum).HasColumnName("stock_minimum").HasPrecision(12, 3).HasDefaultValue(0);
        builder.Property(m => m.Description).HasColumnName("description");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasDefaultValue(true);
        builder.HasOne(m => m.Category).WithMany(c => c.Materials).HasForeignKey(m => m.Macaid);
        builder.HasMany(m => m.Lots).WithOne(l => l.Material).HasForeignKey(l => l.Mateid);
        builder.HasIndex(m => m.Macaid).HasDatabaseName("idx_material_macaid");
    }
}

internal sealed class MaterialLotConfiguration : IEntityTypeConfiguration<MaterialLot>
{
    public void Configure(EntityTypeBuilder<MaterialLot> builder)
    {
        builder.ToTable("material_lot", "maintenance");
        builder.HasKey(ml => ml.Maloid);
        builder.Property(ml => ml.Maloid).HasColumnName("maloid").UseIdentityColumn();
        builder.Property(ml => ml.Mateid).HasColumnName("mateid").IsRequired();
        builder.Property(ml => ml.InitialQuantity).HasColumnName("initial_quantity")
               .HasPrecision(12, 3).IsRequired();
        builder.Property(ml => ml.CurrentQuantity).HasColumnName("current_quantity")
               .HasPrecision(12, 3).IsRequired();
        builder.Property(ml => ml.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 4).HasDefaultValue(0);
        builder.Property(ml => ml.EntryDate).HasColumnName("entry_date")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(ml => ml.ExpirationDate).HasColumnName("expiration_date");
        builder.Property(ml => ml.Provid).HasColumnName("provid");
        builder.Property(ml => ml.SupplierLotNumber).HasColumnName("supplier_lot_number").HasMaxLength(100);
        builder.Property(ml => ml.Note).HasColumnName("note");
        builder.Property(ml => ml.LotStatus).HasColumnName("lot_status").HasMaxLength(20)
               .HasDefaultValue("activo");
        builder.Property(ml => ml.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.HasIndex(ml => ml.Mateid).HasDatabaseName("idx_lot_mateid");
        builder.HasIndex(ml => new { ml.Mateid, ml.LotStatus }).HasDatabaseName("idx_lot_status_mateid");
    }
}

internal sealed class MaterialConsumptionConfiguration : IEntityTypeConfiguration<MaterialConsumption>
{
    public void Configure(EntityTypeBuilder<MaterialConsumption> builder)
    {
        builder.ToTable("material_consumption", "maintenance");
        builder.HasKey(mc => mc.Macoid);
        builder.Property(mc => mc.Macoid).HasColumnName("macoid").UseIdentityColumn();
        builder.Property(mc => mc.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(mc => mc.Mateid).HasColumnName("mateid").IsRequired();
        builder.Property(mc => mc.Maloid).HasColumnName("maloid");
        builder.Property(mc => mc.Quantity).HasColumnName("quantity").HasPrecision(12, 3).IsRequired();
        builder.Property(mc => mc.Origin).HasColumnName("origin").HasMaxLength(50).HasDefaultValue("Stock propio");
        builder.Property(mc => mc.ConsumedAt).HasColumnName("consumed_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasOne(mc => mc.Lot).WithMany().HasForeignKey(mc => mc.Maloid);
        builder.HasIndex(mc => mc.Mainid).HasDatabaseName("idx_consumption_mainid");
        builder.HasIndex(mc => mc.Mateid).HasDatabaseName("idx_consumption_mateid");
        builder.HasIndex(mc => mc.Maloid).HasDatabaseName("idx_consumption_maloid");
    }
}

internal sealed class MaterialDiscardConfiguration : IEntityTypeConfiguration<MaterialDiscard>
{
    public void Configure(EntityTypeBuilder<MaterialDiscard> builder)
    {
        builder.ToTable("material_discard", "maintenance");
        builder.HasKey(md => md.Madiid);
        builder.Property(md => md.Madiid).HasColumnName("madiid").UseIdentityColumn();
        builder.Property(md => md.Maloid).HasColumnName("maloid").IsRequired();
        builder.Property(md => md.DiscardedQuantity).HasColumnName("discarded_quantity")
               .HasPrecision(12, 3).IsRequired();
        builder.Property(md => md.DiscardDate).HasColumnName("discard_date")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(md => md.Reason).HasColumnName("reason").HasMaxLength(50).IsRequired();
        builder.Property(md => md.Note).HasColumnName("note");
        builder.Property(md => md.DiscardedBy).HasColumnName("discarded_by").IsRequired();
        builder.HasOne(md => md.Lot).WithMany().HasForeignKey(md => md.Maloid);
    }
}

internal sealed class MaterialRatingConfiguration : IEntityTypeConfiguration<MaterialRating>
{
    public void Configure(EntityTypeBuilder<MaterialRating> builder)
    {
        builder.ToTable("material_rating", "maintenance");
        builder.HasKey(mr => mr.Matraid);
        builder.Property(mr => mr.Matraid).HasColumnName("matraid").UseIdentityColumn();
        builder.Property(mr => mr.Mateid).HasColumnName("mateid").IsRequired();
        builder.Property(mr => mr.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(mr => mr.Rating).HasColumnName("rating").IsRequired();
        builder.Property(mr => mr.Observation).HasColumnName("observation");
        builder.Property(mr => mr.RatedBy).HasColumnName("rated_by").IsRequired();
        builder.Property(mr => mr.RatedAt).HasColumnName("rated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasIndex(mr => mr.Mateid).HasDatabaseName("idx_rating_mateid");
        builder.HasIndex(mr => mr.Mainid).HasDatabaseName("idx_rating_mainid");
    }
}

internal sealed class InstalledComponentConfiguration : IEntityTypeConfiguration<InstalledComponent>
{
    public void Configure(EntityTypeBuilder<InstalledComponent> builder)
    {
        builder.ToTable("installed_component", "maintenance");
        builder.HasKey(ic => ic.Incoid);
        builder.Property(ic => ic.Incoid).HasColumnName("incoid").UseIdentityColumn();
        builder.Property(ic => ic.Prcoid).HasColumnName("prcoid").IsRequired();
        builder.Property(ic => ic.Acatid).HasColumnName("acatid").IsRequired();
        builder.Property(ic => ic.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(ic => ic.Maloid).HasColumnName("maloid");
        builder.Property(ic => ic.InstallationDate).HasColumnName("installation_date")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(ic => ic.InstallationKm).HasColumnName("installation_km").IsRequired();
        builder.Property(ic => ic.ExpirationDate).HasColumnName("expiration_date");
        builder.Property(ic => ic.Active).HasColumnName("active").HasDefaultValue(true);
        builder.Property(ic => ic.ReplacedByIncoid).HasColumnName("replaced_by_incoid");
        builder.HasOne(ic => ic.ActionCatalog).WithMany().HasForeignKey(ic => ic.Acatid);
        builder.HasOne(ic => ic.Lot).WithMany().HasForeignKey(ic => ic.Maloid);
        builder.HasOne<InstalledComponent>().WithMany().HasForeignKey(ic => ic.ReplacedByIncoid);
        builder.HasIndex(ic => ic.Prcoid).HasFilter("active = true")
               .HasDatabaseName("idx_ic_prcoid_active");
        builder.HasIndex(ic => ic.ExpirationDate)
               .HasFilter("active = true AND expiration_date IS NOT NULL")
               .HasDatabaseName("idx_ic_expiration");
        builder.HasIndex(ic => ic.Acatid).HasDatabaseName("idx_ic_acatid");
    }
}

internal sealed class AlertConfigConfiguration : IEntityTypeConfiguration<AlertConfig>
{
    public void Configure(EntityTypeBuilder<AlertConfig> builder)
    {
        builder.ToTable("alert_config", "maintenance");
        builder.HasKey(ac => ac.Alcoid);
        builder.Property(ac => ac.Alcoid).HasColumnName("alcoid").UseIdentityColumn();
        builder.Property(ac => ac.AlertType).HasColumnName("alert_type").HasMaxLength(50).IsRequired();
        builder.Property(ac => ac.Description).HasColumnName("description");
        builder.Property(ac => ac.Enabled).HasColumnName("enabled").HasDefaultValue(true);
        builder.Property(ac => ac.ThresholdValue).HasColumnName("threshold_value").HasMaxLength(50);
        builder.Property(ac => ac.ThresholdUnit).HasColumnName("threshold_unit").HasMaxLength(30);
        builder.HasIndex(ac => ac.AlertType).IsUnique();
    }
}

internal sealed class AlertLogConfiguration : IEntityTypeConfiguration<AlertLog>
{
    public void Configure(EntityTypeBuilder<AlertLog> builder)
    {
        builder.ToTable("alert_log", "maintenance");
        builder.HasKey(al => al.Alloid);
        builder.Property(al => al.Alloid).HasColumnName("alloid").UseIdentityColumn();
        builder.Property(al => al.Alcoid).HasColumnName("alcoid").IsRequired();
        builder.Property(al => al.Prcoid).HasColumnName("prcoid");
        builder.Property(al => al.Mateid).HasColumnName("mateid");
        builder.Property(al => al.Maloid).HasColumnName("maloid");
        builder.Property(al => al.Incoid).HasColumnName("incoid");
        builder.Property(al => al.Message).HasColumnName("message").IsRequired();
        builder.Property(al => al.AlertDate).HasColumnName("alert_date")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(al => al.Read).HasColumnName("read").HasDefaultValue(false);
        builder.Property(al => al.ReadAt).HasColumnName("read_at");
        builder.Property(al => al.ReadBy).HasColumnName("read_by");
        builder.Property(al => al.Resolved).HasColumnName("resolved").HasDefaultValue(false);
        builder.Property(al => al.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(al => al.ResolvedBy).HasColumnName("resolved_by");
        builder.HasOne(al => al.AlertConfig).WithMany().HasForeignKey(al => al.Alcoid);
        builder.HasIndex(al => al.Read).HasFilter("read = false").HasDatabaseName("idx_alert_unread");
        builder.HasIndex(al => al.Prcoid).HasDatabaseName("idx_alert_prcoid");
        builder.HasIndex(al => al.AlertDate).HasDatabaseName("idx_alert_date");
    }
}

internal sealed class ConfigSystemConfiguration : IEntityTypeConfiguration<ConfigSystem>
{
    public void Configure(EntityTypeBuilder<ConfigSystem> builder)
    {
        builder.ToTable("config_system", "maintenance");
        builder.HasKey(cs => cs.Cosyid);
        builder.Property(cs => cs.Cosyid).HasColumnName("cosyid").UseIdentityColumn();
        builder.Property(cs => cs.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        builder.Property(cs => cs.Value).HasColumnName("value").HasMaxLength(255).IsRequired();
        builder.Property(cs => cs.Description).HasColumnName("description");
        builder.Property(cs => cs.DataType).HasColumnName("data_type").HasMaxLength(20)
               .HasDefaultValue("string");
        builder.Property(cs => cs.UpdatedAt).HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(cs => cs.UpdatedBy).HasColumnName("updated_by");
        builder.Property(cs => cs.Status).HasColumnName("status").HasDefaultValue(true);
        builder.HasIndex(cs => cs.Key).IsUnique().HasDatabaseName("config_system_key_unique");
    }
}

/// <summary>
/// Tabla technician_assignment agregada por 02_ajustes_fase1.sql [A4].
/// EF Core la mapea normalmente — la tabla existe en BD tras aplicar ajustes.
/// </summary>
internal sealed class TechnicianAssignmentConfiguration : IEntityTypeConfiguration<TechnicianAssignment>
{
    public void Configure(EntityTypeBuilder<TechnicianAssignment> builder)
    {
        builder.ToTable("technician_assignment", "maintenance");
        builder.HasKey(ta => ta.Teasid);
        builder.Property(ta => ta.Teasid).HasColumnName("teasid").UseIdentityColumn();
        builder.Property(ta => ta.Mainid).HasColumnName("mainid").IsRequired();
        builder.Property(ta => ta.Workid).HasColumnName("workid").IsRequired();
        builder.Property(ta => ta.RoleInJob).HasColumnName("role_in_job").HasMaxLength(50)
               .HasDefaultValue("Principal");
        builder.Property(ta => ta.AssignedAt).HasColumnName("assigned_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(ta => ta.AssignedBy).HasColumnName("assigned_by").IsRequired();
        builder.HasIndex(ta => new { ta.Mainid, ta.Workid }).IsUnique()
               .HasDatabaseName("ta_mainid_workid_unique");
        builder.HasIndex(ta => ta.Mainid).HasDatabaseName("idx_ta_mainid");
        builder.HasIndex(ta => ta.Workid).HasDatabaseName("idx_ta_workid");
    }
}