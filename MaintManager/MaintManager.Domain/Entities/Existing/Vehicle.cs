// MaintManager.Domain/Entities/Existing/Vehicle.cs
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
