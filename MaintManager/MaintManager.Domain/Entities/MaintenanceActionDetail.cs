namespace MaintManager.Domain.Entities;

public sealed class MaintenanceActionDetail
{
    public int Madeid { get; private set; }
    public int Mainid { get; private set; }
    public int Acatid { get; private set; }
    public bool Completed { get; private set; }
    public char? ActionPerformed { get; private set; }
    public string? ProductUsed { get; private set; }
    public string? QuantityUsed { get; private set; }
    public string? OriginProduct { get; private set; }
    public string? Observation { get; private set; }
    public int? Maloid { get; private set; }

    // Navegación
    public ActionCatalog? ActionCatalog { get; private set; }
    public MaterialLot? MaterialLot { get; private set; }

    private MaintenanceActionDetail() { }

    public static MaintenanceActionDetail Create(int mainid, int acatid) =>
        new() { Mainid = mainid, Acatid = acatid, Completed = false };

    public void Complete(char actionCode, string? productUsed,
        string? quantityUsed, string? originProduct, string? observation, int? maloid = null)
    {
        Completed = true;
        ActionPerformed = actionCode;
        ProductUsed = productUsed;
        QuantityUsed = quantityUsed;
        OriginProduct = originProduct;
        Observation = observation;
        Maloid = maloid;
    }
}


// MaintManager.Domain/Entities/Diagnosis.cs

/// <summary>Diagnóstico final del mecánico al cerrar un mantenimiento.</summary>
