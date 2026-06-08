namespace MaintManager.MAUI.Models;

public partial class AlertItem
{
    public int Alloid { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? LicensePlate { get; set; }
    public string? MaterialName { get; set; }
    public bool IsRead { get; set; }
    public bool IsResolved { get; set; }

    public string LevelLabel => IsResolved ? "Resuelta" : IsRead ? "Leída" : "Nueva";
    public string LevelColor => IsResolved ? "#4CAF50" : IsRead ? "#2196F3" : "#FF9800";
    public string CreatedAtFormatted => $"Creada: {AlertDate:dd/MM/yyyy HH:mm}" + (ReadAt.HasValue ? $" · Leída: {ReadAt:dd/MM/yyyy HH:mm}" : "");
}
