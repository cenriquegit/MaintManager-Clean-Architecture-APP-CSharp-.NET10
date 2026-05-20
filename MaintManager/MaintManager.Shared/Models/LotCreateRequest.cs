namespace MaintManager.Shared.Models;

public sealed record LotCreateRequest(
    int Mateid,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpirationDate,
    int? Provid,
    string? SupplierLotNumber,
    string? Note
);
