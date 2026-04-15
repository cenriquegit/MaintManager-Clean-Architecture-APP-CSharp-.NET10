public static class VehicleMappings
{
    public static VehicleListItem ToListItem(this Vehicle v, int currentKm,
        int? nextMaintenanceKm, bool isDueSoon) =>
        new(
            Prcoid: v.Prcoid,
            LicensePlate: v.LicensePlateNumber,
            VehicleName: v.Product?.Name ?? "Sin nombre",
            CurrentKm: currentKm,
            NextMaintenanceKm: nextMaintenanceKm,
            IsMaintenanceDueSoon: isDueSoon
        );

    public static VehicleResponse ToResponse(this Vehicle v, int currentKm,
        int? nextKm, int? alertThreshold, bool isDueSoon) =>
        new(
            Prcoid: v.Prcoid,
            LicensePlate: v.LicensePlateNumber,
            VinNumber: v.VinNumber,
            VehicleName: v.Product?.Name ?? "Sin nombre",
            VehicleType: v.Vetyid,
            FuelType: v.Futyid,
            Year: v.YearOfManufacture,
            Color: v.Color,
            Category: v.Category,
            CurrentKm: currentKm,
            NextMaintenanceKm: nextKm,
            KmUntilNextService: nextKm.HasValue ? nextKm.Value - currentKm : null,
            IsMaintenanceDueSoon: isDueSoon,
            IsActive: v.Status
        );
}

/// <summary>Mapeos de Maintenance → DTO.</summary>
