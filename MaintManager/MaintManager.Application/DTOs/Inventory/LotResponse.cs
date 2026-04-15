namespace MaintManager.Application.DTOs.Inventory;

public sealed record LotResponse(
    int Maloid,
    decimal InitialQuantity,
    decimal CurrentQuantity,
    decimal UnitCost,
    DateTime EntryDate,
    DateOnly? ExpirationDate,
    int? DaysUntilExpiry,
    string LotStatus
);

/// <summary>Descartar cantidad de un lote.</summary>
