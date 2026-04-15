namespace MaintManager.Domain.Entities;

public sealed class Diagnosis
{
    public int Diagid { get; private set; }
    public int Mainid { get; private set; }
    public string GeneralStatus { get; private set; } = string.Empty;
    public string? Observations { get; private set; }
    public bool VehicleOperative { get; private set; } = true;
    public string? FutureRecommendations { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Diagnosis() { }

    public static Diagnosis Create(int mainid, string generalStatus,
        bool vehicleOperative, string? observations = null, string? futureRecommendations = null)
    {
        if (string.IsNullOrWhiteSpace(generalStatus))
            throw new ArgumentException("El estado general del diagnóstico es obligatorio.", nameof(generalStatus));

        return new Diagnosis
        {
            Mainid = mainid,
            GeneralStatus = generalStatus,
            VehicleOperative = vehicleOperative,
            Observations = observations,
            FutureRecommendations = futureRecommendations,
            CreatedAt = DateTime.UtcNow
        };
    }
}
