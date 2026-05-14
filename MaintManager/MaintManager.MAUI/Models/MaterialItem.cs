namespace MaintManager.MAUI.Models;

public partial class MaterialItem
{
    public int Mateid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal StockTotal { get; set; }
    public decimal StockMinimum { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool IsBelowMinimum { get; set; }
}
