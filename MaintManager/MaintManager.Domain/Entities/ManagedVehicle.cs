namespace MaintManager.Domain.Entities;

public sealed class ManagedVehicle
{
    public int MvId { get; private set; }
    public int? Prcoid { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;
    public string VehicleName { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public short? Year { get; private set; }
    public string? Color { get; private set; }
    public string? Vin { get; private set; }
    public string? EngineNumber { get; private set; }
    public string Source { get; private set; } = "managed";
    public bool Status { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ManagedVehicle() { }

    public static ManagedVehicle Create(string licensePlate, string vehicleName,
        string? brand = null, string? model = null, short? year = null,
        string? color = null, string? vin = null, string? engineNumber = null,
        string source = "managed") => new()
    {
        LicensePlate = licensePlate,
        VehicleName = vehicleName,
        Brand = brand,
        Model = model,
        Year = year,
        Color = color,
        Vin = vin,
        EngineNumber = engineNumber,
        Source = source,
        Status = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public void Update(string vehicleName, string? brand, string? model,
        short? year, string? color, string? vin, string? engineNumber)
    {
        VehicleName = vehicleName;
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Vin = vin;
        EngineNumber = engineNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncLegacyFields(string? brand, string? model, short? year,
        string? color, string? vin, string? engineNumber)
    {
        Brand = brand ?? Brand;
        Model = model ?? Model;
        Year = year ?? Year;
        Color = color ?? Color;
        Vin = vin ?? Vin;
        EngineNumber = engineNumber ?? EngineNumber;
        UpdatedAt = DateTime.UtcNow;
    }
}
