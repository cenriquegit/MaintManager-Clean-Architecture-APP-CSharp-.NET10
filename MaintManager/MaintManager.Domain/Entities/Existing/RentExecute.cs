// MaintManager.Domain/Entities/Existing/RentExecute.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class RentExecute
{
    public int Seexid { get; init; }
    public int? KilometerStart { get; init; }
    public int? KilometerEnd { get; init; }
    public DateTime? ReturnDate { get; init; }
    public string? Statid { get; init; }
    public int? Sereid { get; init; }
    public RentRequest? RentRequest { get; init; }
}
