
/// <summary>
/// Componente actualmente instalado en un vehículo.
/// Permite rastrear caducidad por tiempo (ej: aceite motor = 12 meses).
/// </summary>
namespace MaintManager.Domain.Entities;

public sealed class InstalledComponent
{
    public int Incoid { get; private set; }
    public int Prcoid { get; private set; }
    public int Acatid { get; private set; }
    public int Mainid { get; private set; }
    public int? Maloid { get; private set; }
    public DateTime InstallationDate { get; private set; }
    public int InstallationKm { get; private set; }

    /// <summary>Calculado: InstallationDate + UsefulLifeDays del catálogo. Null si no caduca.</summary>
    public DateOnly? ExpirationDate { get; private set; }

    public bool Active { get; private set; } = true;

    /// <summary>ID del componente que lo reemplazó (historial de sustituciones).</summary>
    public int? ReplacedByIncoid { get; private set; }

    // Navegación
    public ActionCatalog? ActionCatalog { get; private set; }
    public MaterialLot? Lot { get; private set; }

    private InstalledComponent() { }

    public static InstalledComponent Create(int prcoid, int acatid, int mainid,
        int installationKm, int? maloid = null, int? usefulLifeDays = null)
    {
        if (installationKm < 0)
            throw new ArgumentException("El km de instalación no puede ser negativo.", nameof(installationKm));

        DateOnly? expirationDate = null;
        if (usefulLifeDays.HasValue && usefulLifeDays > 0)
            expirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(usefulLifeDays.Value));

        return new InstalledComponent
        {
            Prcoid = prcoid,
            Acatid = acatid,
            Mainid = mainid,
            Maloid = maloid,
            InstallationDate = DateTime.UtcNow,
            InstallationKm = installationKm,
            ExpirationDate = expirationDate,
            Active = true
        };
    }

    /// <summary>Desactiva el componente al ser reemplazado por uno nuevo.</summary>
    public void Replace(int newComponentId)
    {
        Active = false;
        ReplacedByIncoid = newComponentId;
    }
}
