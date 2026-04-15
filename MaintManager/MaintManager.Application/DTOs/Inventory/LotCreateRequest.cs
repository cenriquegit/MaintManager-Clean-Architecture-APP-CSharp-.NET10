namespace MaintManager.Application.DTOs.Inventory;

public sealed record LotCreateRequest(
    int Mateid,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpirationDate,
    int? Provid,
    string? SupplierLotNumber,
    string? Note
);

/// <summary>Lote en respuesta.</summary>
