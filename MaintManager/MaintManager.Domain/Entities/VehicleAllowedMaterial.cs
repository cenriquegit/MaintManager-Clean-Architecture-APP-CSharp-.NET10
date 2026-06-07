namespace MaintManager.Domain.Entities;

public sealed class VehicleAllowedMaterial
{
    public int Vamid { get; private set; }
    public int? Prcoid { get; private set; }
    public int? MvId { get; private set; }
    public int Mateid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VehicleAllowedMaterial() { }

    public static VehicleAllowedMaterial Create(int? prcoid, int mateid, int? mvId = null) => new()
    {
        Prcoid = prcoid,
        MvId = mvId,
        Mateid = mateid,
        CreatedAt = DateTime.UtcNow
    };
}
