namespace MaintManager.Domain.Entities;

public sealed class VehicleAllowedAction
{
    public int Vaacid { get; private set; }
    public int? Prcoid { get; private set; }
    public int? MvId { get; private set; }
    public int Acatid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VehicleAllowedAction() { }

    public static VehicleAllowedAction Create(int? prcoid, int acatid, int? mvId = null) => new()
    {
        Prcoid = prcoid,
        MvId = mvId,
        Acatid = acatid,
        CreatedAt = DateTime.UtcNow
    };
}
