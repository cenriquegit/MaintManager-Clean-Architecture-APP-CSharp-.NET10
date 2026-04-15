namespace MaintManager.Application.DTOs.Inventory;

public sealed record MaterialCreateRequest(
    short Macaid,
    string Name,
    string UnitOfMeasure,
    decimal StockMinimum,
    string? Description
);

/// <summary>Material en respuesta.</summary>
