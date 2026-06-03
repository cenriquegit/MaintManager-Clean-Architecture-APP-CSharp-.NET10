using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class LotListViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _mateid;

    public LotListViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Lotes";
    }

    [ObservableProperty]
    private string _materialName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LotItem> _lots = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mateid", out var id))
            _mateid = Convert.ToInt32(id);
        if (query.TryGetValue("name", out var name))
            MaterialName = name?.ToString() ?? "Lotes";
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Inventory.GetMaterialLots.Replace("{id}", _mateid.ToString());
            var response = await _apiService.GetAsync<ApiResponse<List<LotItem>>>(endpoint);
            if (response?.Success == true)
            {
                Lots = new ObservableCollection<LotItem>(response.Data ?? new List<LotItem>());
                IsEmpty = Lots.Count == 0;
            }
            else
            {
                throw new Exception(response?.Message ?? "Error al cargar lotes");
            }
        });
    }

    [RelayCommand]
    private async Task DiscardLot(LotItem lot)
    {
        var confirm = await Shell.Current.DisplayAlert("Descartar lote",
            $"¿Descartar \"{lot.LotNumberDisplay}\"?\n" +
            $"Cantidad: {lot.CurrentQuantity:N1}\n" +
            "Se marcará como descartado y no se podrá consumir.",
            "Sí, descartar", "Cancelar");
        if (!confirm) return;

        var reason = await Shell.Current.DisplayActionSheet(
            "Motivo del descarte",
            "Cancelar", null,
            "Vencimiento", "Daño", "Otro");
        if (string.IsNullOrWhiteSpace(reason) || reason == "Cancelar") return;

        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Inventory.DiscardLot.Replace("{lotId}", lot.Maloid.ToString());
            await _apiService.PostAsync<object>(endpoint, new
            {
                Quantity = lot.CurrentQuantity,
                Reason = reason,
                Note = (string?)null
            });
            await Load();
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}

public class LotItem
{
    public int Maloid { get; set; }
    public int Mateid { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal InitialQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime EntryDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? SupplierLotNumber { get; set; }
    public string LotStatus { get; set; } = "activo";

    public string StatusDisplay => LotStatus switch
    {
        "activo" => "✅ Activo",
        "agotado" => "❌ Agotado",
        "vencido" => "⚠️ Vencido",
        "descartado" => "🗑️ Descartado",
        _ => LotStatus
    };

    public string EntryDateDisplay => EntryDate.ToString("dd/MM/yyyy");
    public string ExpirationDisplay => ExpirationDate?.ToString("dd/MM/yyyy") ?? "Sin vencimiento";
    public string CostDisplay => $"S/ {UnitCost:N2}";
    public string QuantityDisplay => $"{CurrentQuantity:N1} / {InitialQuantity:N1}";
    public string LotNumberDisplay => string.IsNullOrEmpty(SupplierLotNumber) ? "S/N" : SupplierLotNumber;
    public bool CanDiscard => LotStatus == "activo";
}
