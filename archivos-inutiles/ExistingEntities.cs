// MaintManager.Domain/Entities/Existing/ExistingEntities.cs
// ENTIDADES SCAFFOLDED — Solo lectura. Reflejan la BD-FINAL completamente corregida.
// No modificar estructura. No agregar lógica de negocio.
namespace MaintManager.Domain.Entities.Existing;

/// <summary>Vehículo de la flota. Mapea: product.vehicle (hereda de product.company).</summary>
public sealed class Vehicle
{
    public int Prcoid { get; init; }
    public int Prodid { get; init; }
    public string? LicensePlateNumber { get; init; }
    public string? VinNumber { get; init; }
    public string? Vetyid { get; init; }
    public short? YearOfManufacture { get; init; }
    public string? EngineNumber { get; init; }
    public string? Futyid { get; init; }
    public string? Color { get; init; }
    public int? Mileage { get; init; }
    public string? Category { get; init; }
    public bool Status { get; init; }
    public Product? Product { get; init; }
}

/// <summary>Producto base. Mapea: public.product.</summary>
public sealed class Product
{
    public int Prodid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
}

/// <summary>
/// Trabajador / usuario del sistema. Mapea: public.worker.
/// username + password (MD5) se usan para autenticación JWT.
/// </summary>
public sealed class Worker
{
    public int Workid { get; init; }
    public int Persid { get; init; }
    public short Jobid { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? Email { get; init; }
    public bool Status { get; init; }
    public bool Locked { get; init; }
    public Person? Person { get; init; }
}

/// <summary>Persona natural. Mapea: public.person.</summary>
public sealed class Person
{
    public int Persid { get; init; }
    public string Fln { get; init; } = string.Empty;
    public string? Mln { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Empresa jurídica. Mapea: public.company.
/// Referenciada por company.worker, public.client, public.provider.
/// </summary>
public sealed class Company
{
    public int Compid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Ruc { get; init; } = string.Empty;
    public string? TradeName { get; init; }
    public bool Status { get; init; }
}

/// <summary>
/// Zona geográfica. Mapea: public.zone.
/// Referenciada por public.agency.
/// </summary>
public sealed class Zone
{
    public short Zoneid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
}

/// <summary>
/// Agencia / sucursal. Mapea: public.agency.
/// Requerida en public.client (agenid NOT NULL).
/// </summary>
public sealed class Agency
{
    public short Agenid { get; init; }
    public short Zoneid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
    public Zone? Zone { get; init; }
}

/// <summary>
/// Dirección / residencia. Mapea: public.residence.
/// PK: resiid (nextval de public.address_addrid_seq).
/// </summary>
public sealed class Residence
{
    public int Resiid { get; init; }
    public int? Persid { get; init; }
    public int? Compid { get; init; }
    public int Distid { get; init; }
    public string? Address { get; init; }
    public bool Status { get; init; }
}

/// <summary>
/// Ejecución de renta de vehículo. Mapea: service.rentexecute.
/// kilometer_end = fuente del km actual real del vehículo.
/// </summary>
public sealed class RentExecute
{
    public int Seexid { get; init; }
    public int? KilometerStart { get; init; }
    public int? KilometerEnd { get; init; }
    public DateTime? ReturnDate { get; init; }
    public string? Statid { get; init; }
    public int? Sereid { get; init; }
    public RentRequest? RentRequest { get; init; }
}

/// <summary>Solicitud de renta. Mapea: service.rentrequest.</summary>
public sealed class RentRequest
{
    public int Sereid { get; init; }
    public int Prodid { get; init; }
    public string? Statid { get; init; }
}

/// <summary>
/// Trabajador de empresa cliente. Mapea: company.worker.
/// Usado en rentexecute (received_cowoid, delivery_cowoid).
/// FK a public.company incluida [F8].
/// </summary>
public sealed class CompanyWorker
{
    public int Cowoid { get; init; }
    public int Compid { get; init; }
    public int Persid { get; init; }
    public bool Status { get; init; }
    public Company? Company { get; init; }
    public Person? Person { get; init; }
}