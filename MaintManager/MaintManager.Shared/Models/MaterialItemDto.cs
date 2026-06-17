namespace MaintManager.Shared.Models;

public sealed record MaterialItemDto(int Mateid, string Name, string UnitOfMeasure, decimal StockTotal = 0);
