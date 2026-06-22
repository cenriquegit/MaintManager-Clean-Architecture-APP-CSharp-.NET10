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
    IReadOnlyList<LotResponse> ActiveLots,
    MaterialRatingInfo? LastRating,
    string Type = "Material"
);

public sealed record MaterialRatingInfo(
    short Rating,
    string? Observation,
    DateTime RatedAt
);

/// <summary>Elemento simplificado para listados.</summary>
