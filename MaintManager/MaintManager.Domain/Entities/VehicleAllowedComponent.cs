namespace MaintManager.Domain.Entities;

public sealed class VehicleAllowedComponent
{
    public int Vacoid { get; private set; }
    public int? Prcoid { get; private set; }
    public int? MvId { get; private set; }
    public int Acatid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VehicleAllowedComponent() { }

    public static VehicleAllowedComponent Create(int? prcoid, int acatid, int? mvId = null) => new()
    {
        Prcoid = prcoid,
        MvId = mvId,
        Acatid = acatid,
        CreatedAt = DateTime.UtcNow
    };
}
