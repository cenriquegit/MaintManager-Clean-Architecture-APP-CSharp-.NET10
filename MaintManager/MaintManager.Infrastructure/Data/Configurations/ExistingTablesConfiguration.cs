// MaintManager.Infrastructure/Data/Configurations/ExistingTablesConfiguration.cs
// Configuraciones para tablas existentes de la empresa.
// Todas marcadas con ExcludeFromMigrations() — nunca se tocan desde EF Core.
// Refleja la BD-FINAL completamente corregida (F1-F11 aplicados).
using MaintManager.Domain.Entities.Existing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintManager.Infrastructure.Data.Configurations;

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
        builder.HasOne(v => v.Product).WithMany().HasForeignKey(v => v.Prodid);
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
        builder.HasOne(w => w.Person).WithMany().HasForeignKey(w => w.Persid);
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

/// <summary>
/// public.company — empresa jurídica.
/// Agregada en BD-FINAL [F8] con FK a list.companyconditionlist, companystatuslist, taxpayertypelist.
/// Solo mapeamos campos que el sistema de mantenimiento necesita leer.
/// </summary>
internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company", "public");
        builder.HasKey(c => c.Compid);
        builder.Property(c => c.Compid).HasColumnName("compid");
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(c => c.Ruc).HasColumnName("ruc").HasMaxLength(11);
        builder.Property(c => c.TradeName).HasColumnName("tradename").HasMaxLength(200);
        builder.Property(c => c.Status).HasColumnName("status");
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

/// <summary>public.zone — zona geográfica. Requerida por public.agency.</summary>
internal sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zone", "public");
        builder.HasKey(z => z.Zoneid);
        builder.Property(z => z.Zoneid).HasColumnName("zoneid");
        builder.Property(z => z.Name).HasColumnName("name").HasMaxLength(255);
        builder.Property(z => z.Status).HasColumnName("status");
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

/// <summary>
/// public.agency — agencia/sucursal. [F10] nextval con esquema explícito.
/// Requerida por public.client (agenid NOT NULL — FK que faltaba en BD anterior).
/// El seed data necesita crear un registro de agencia antes de crear clientes.
/// </summary>
internal sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agency", "public");
        builder.HasKey(a => a.Agenid);
        builder.Property(a => a.Agenid).HasColumnName("agenid");
        builder.Property(a => a.Zoneid).HasColumnName("zoneid");
        builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(255);
        builder.Property(a => a.Status).HasColumnName("status");
        builder.HasOne(a => a.Zone).WithMany().HasForeignKey(a => a.Zoneid);
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}

/// <summary>
/// public.residence — dirección. [F9] PK usa public.address_addrid_seq.
/// Tabla renombrada de 'address' a 'residence' en BD-FINAL.
/// </summary>
internal sealed class ResidenceConfiguration : IEntityTypeConfiguration<Residence>
{
    public void Configure(EntityTypeBuilder<Residence> builder)
    {
        builder.ToTable("residence", "public");
        builder.HasKey(r => r.Resiid);
        builder.Property(r => r.Resiid).HasColumnName("resiid");
        builder.Property(r => r.Persid).HasColumnName("persid");
        builder.Property(r => r.Compid).HasColumnName("compid");
        builder.Property(r => r.Distid).HasColumnName("distid");
        builder.Property(r => r.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(r => r.Status).HasColumnName("status");
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
        builder.HasOne(r => r.RentRequest).WithMany().HasForeignKey(r => r.Sereid);
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

/// <summary>
/// company.worker — trabajador de empresa cliente. [F8] FK a public.company re-incluida.
/// Usado en service.rentexecute (received_cowoid, delivery_cowoid).
/// </summary>
internal sealed class CompanyWorkerConfiguration : IEntityTypeConfiguration<CompanyWorker>
{
    public void Configure(EntityTypeBuilder<CompanyWorker> builder)
    {
        builder.ToTable("worker", "company");
        builder.HasKey(cw => cw.Cowoid);
        builder.Property(cw => cw.Cowoid).HasColumnName("cowoid");
        builder.Property(cw => cw.Compid).HasColumnName("compid");
        builder.Property(cw => cw.Persid).HasColumnName("persid");
        builder.Property(cw => cw.Status).HasColumnName("status");
        builder.HasOne(cw => cw.Company).WithMany().HasForeignKey(cw => cw.Compid);
        builder.HasOne(cw => cw.Person).WithMany().HasForeignKey(cw => cw.Persid);
        builder.ToTable(t => t.ExcludeFromMigrations());
    }
}