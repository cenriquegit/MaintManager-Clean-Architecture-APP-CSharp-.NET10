namespace MaintManager.MAUI.Models;

public partial class MaintenanceCalendarItem
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string TypeColor => Type switch
    {
        "Emergencia" => "#E53935",
        "Programado" => "#1E88E5",
        "Completado" => "#2ECC71",
        "Preventivo" => "#F57C00",
        _ => "#546E8A",
    };
    public string FormattedDate => ScheduledDate.ToString("dd MMM yyyy");
}
