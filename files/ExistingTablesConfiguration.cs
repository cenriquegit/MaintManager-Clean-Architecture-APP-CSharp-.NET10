// MaintManager.Infrastructure/Data/Configurations/ExistingTablesConfiguration.cs
using MaintManager.Domain.Entities.Existing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración de tablas existentes de la empresa.
/// Solo lectura — nunca se modifican desde este sistema.
/// </summary>
internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicle", "product");
        builder.HasKey(v => v.Prcoid);

        builder.Property(v => v.Prcoid).HasColumnName("prcoid");
        builder.Property(v => v.Prodid).HasColumnName("prodid");
        builder.Property(v => v.LicensePlateNumber).HasColumnName("license_plate_number").HasMaxLength(50);
        builder.Property(v => v.VinNumber).HasColumnName("vin_number").HasMaxLength(50);
        builder.Property(v => v.Vetyid).HasColumnName("vetyid").HasMaxLength(2);
        builder.Property(v => v.YearOfManufacture).HasColumnName("year_of_manufacture");
        builder.Property(v => v.EngineNumber).HasColumnName("engine_number").HasMaxLength(50);
        builder.Property(v => v.Futyid).HasColumnName("futyid").HasMaxLength(2);
        builder.Property(v => v.Color).HasColumnName("color").HasMaxLength(35);
        builder.Property(v => v.Mileage).HasColumnName("mileage");
        builder.Property(v => v.Category).HasColumnName("category").HasMaxLength(50);
        builder.Property(v => v.Status).HasColumnName("status");

        builder.HasOne(v => v.Product)
               .WithMany()
               .HasForeignKey(v => v.Prodid);

        // Solo lectura — nunca migrar esta tabla
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product", "public");
        builder.HasKey(p => p.Prodid);
        builder.Property(p => p.Prodid).HasColumnName("prodid");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(255);
        builder.Property(p => p.Status).HasColumnName("status");
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

internal sealed class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
    public void Configure(EntityTypeBuilder<Worker> builder)
    {
        builder.ToTable("worker", "public");
        builder.HasKey(w => w.Workid);
        builder.Property(w => w.Workid).HasColumnName("workid");
        builder.Property(w => w.Persid).HasColumnName("persid");
        builder.Property(w => w.Jobid).HasColumnName("jobid");
        builder.Property(w => w.Username).HasColumnName("username").HasMaxLength(25);
        builder.Property(w => w.Password).HasColumnName("password").HasMaxLength(32);
        builder.Property(w => w.Email).HasColumnName("email").HasMaxLength(255);
        builder.Property(w => w.Status).HasColumnName("status");
        builder.Property(w => w.Locked).HasColumnName("locked");

        builder.HasOne(w => w.Person)
               .WithMany()
               .HasForeignKey(w => w.Persid);

        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("person", "public");
        builder.HasKey(p => p.Persid);
        builder.Property(p => p.Persid).HasColumnName("persid");
        builder.Property(p => p.Fln).HasColumnName("fln").HasMaxLength(50);
        builder.Property(p => p.Mln).HasColumnName("mln").HasMaxLength(50);
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(100);
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

internal sealed class RentExecuteConfiguration : IEntityTypeConfiguration<RentExecute>
{
    public void Configure(EntityTypeBuilder<RentExecute> builder)
    {
        builder.ToTable("rentexecute", "service");
        builder.HasKey(r => r.Seexid);
        builder.Property(r => r.Seexid).HasColumnName("seexid");
        builder.Property(r => r.KilometerStart).HasColumnName("kilometer_start");
        builder.Property(r => r.KilometerEnd).HasColumnName("kilometer_end");
        builder.Property(r => r.ReturnDate).HasColumnName("return_date");
        builder.Property(r => r.Statid).HasColumnName("statid").HasMaxLength(2);
        builder.Property(r => r.Sereid).HasColumnName("sereid");

        builder.HasOne(r => r.RentRequest)
               .WithMany()
               .HasForeignKey(r => r.Sereid);

        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

internal sealed class RentRequestConfiguration : IEntityTypeConfiguration<RentRequest>
{
    public void Configure(EntityTypeBuilder<RentRequest> builder)
    {
        builder.ToTable("rentrequest", "service");
        builder.HasKey(r => r.Sereid);
        builder.Property(r => r.Sereid).HasColumnName("sereid");
        builder.Property(r => r.Prodid).HasColumnName("prodid");
        builder.Property(r => r.Statid).HasColumnName("statid").HasMaxLength(2);
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}
