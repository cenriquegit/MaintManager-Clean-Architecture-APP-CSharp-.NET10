namespace MaintManager.Application.DTOs.Inventory;

public sealed record MaterialListItem(
    int Mateid,
    string Category,
    string Name,
    string UnitOfMeasure,
    decimal StockTotal,
    decimal StockMinimum,
    bool IsBelowMinimum
);

/// <summary>Ingresar un nuevo lote de material.</summary>
