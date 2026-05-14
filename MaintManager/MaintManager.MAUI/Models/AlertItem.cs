namespace MaintManager.MAUI.Models;

public partial class AlertItem
{
    public int Alloid { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public string? LicensePlate { get; set; }
    public string? MaterialName { get; set; }
    public bool IsRead { get; set; }
    public bool Resolved { get; set; }
}
