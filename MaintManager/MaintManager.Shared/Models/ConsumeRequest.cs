namespace MaintManager.Shared.Models;

public sealed record ConsumeRequest(
    int Mateid,
    decimal Quantity
);
