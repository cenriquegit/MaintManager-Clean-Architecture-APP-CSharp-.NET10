namespace MaintManager.Application.DTOs.Inventory;

public sealed record MaterialRatingRequest(
    int Mateid,
    int Mainid,
    short Rating,
    string? Observation
);

