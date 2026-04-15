
namespace MaintManager.Application.DTOs.Inventory;

public sealed record LotDiscardRequest(
    decimal Quantity,
    string Reason,
    string? Note
);

/// <summary>Calificar un material.</summary>
