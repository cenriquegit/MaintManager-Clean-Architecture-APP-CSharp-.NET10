namespace MaintManager.Application.DTOs.Inventory;

public sealed record MaterialResponse(
    int Mateid,
    string Category,
    string Name,
    string UnitOfMeasure,
    decimal StockTotal,
    decimal StockMinimum,
    bool IsBelowMinimum,
    string? Description,
    IReadOnlyList<LotResponse> ActiveLots
);

/// <summary>Elemento simplificado para listados.</summary>
