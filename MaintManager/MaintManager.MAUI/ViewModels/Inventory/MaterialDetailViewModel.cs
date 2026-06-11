using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class MaterialDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _mateid;

    public MaterialDetailViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Detalle Material";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mateid", out var id) && id is string idStr && int.TryParse(idStr, out var mateid))
            _mateid = mateid;
    }

    [ObservableProperty]
    private string _materialName = string.Empty;

    [ObservableProperty]
    private string _unitOfMeasure = string.Empty;

    [ObservableProperty]
    private decimal _stockTotal;

    [ObservableProperty]
    private decimal _stockMinimum;

    [ObservableProperty]
    private ObservableCollection<LotItem> _lots = new();

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var raw = await _apiService.GetAsync<ApiResponse<MaterialInfo>>($"api/v1/inventory/materials/{_mateid}");
            if (raw?.Success == true && raw.Data is not null)
            {
                MaterialName = raw.Data.Name;
                UnitOfMeasure = raw.Data.UnitOfMeasure;
                StockTotal = raw.Data.StockTotal;
                StockMinimum = raw.Data.StockMinimum;
                Title = raw.Data.Name;
            }
        });
    }

    [RelayCommand]
    private async Task NewLot()
    {
        await Shell.Current.GoToAsync($"///Inventory/CreateLot?mateid={_mateid}");
    }

    [RelayCommand]
    private async Task EditMaterial()
    {
        await Shell.Current.GoToAsync($"///Inventory/CreateMaterial?mateid={_mateid}");
    }

    [RelayCommand]
    private async Task DeleteMaterial()
    {
        var confirm = await Shell.Current.DisplayAlert("Eliminar", "¿Desactivar este material?", "Sí", "No");
        if (!confirm) return;
        await _apiService.DeleteAsync($"api/v1/inventory/materials/{_mateid}");
        await Shell.Current.GoToAsync("..");
    }

    public void OnAppearing() => LoadCommand.Execute(null);

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public class MaterialInfo
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal StockTotal { get; set; }
        public decimal StockMinimum { get; set; }
    }

    public class LotItem
    {
        public int Maloid { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
