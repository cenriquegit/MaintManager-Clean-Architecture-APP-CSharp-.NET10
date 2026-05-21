namespace MaintManager.Application.DTOs.Inventory;

public sealed record LotResponse(
    int Maloid,
    int Mateid,
    decimal CurrentQuantity,
    decimal InitialQuantity,
    decimal UnitCost,
    DateTime EntryDate,
    DateOnly? ExpirationDate,
    int? DaysUntilExpiry,
    string? SupplierLotNumber,
    string LotStatus
);
