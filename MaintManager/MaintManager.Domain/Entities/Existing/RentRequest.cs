// MaintManager.Domain/Entities/Existing/RentRequest.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>Solicitud de renta. Mapea: service.rentrequest.</summary>
public sealed class RentRequest
{
    public int Sereid { get; init; }
    public int Prodid { get; init; }
    public string? Statid { get; init; }
}
