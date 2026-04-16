using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class InventoryListViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public InventoryListViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Inventario";
    }

    [ObservableProperty]
    private ObservableCollection<MaterialItem> _materials = new();

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<MaterialItem>>>("api/v1/inventory/materials");
            if (response?.Success == true && response.Data != null)
            {
                Materials = new ObservableCollection<MaterialItem>(response.Data);
            }
        });
    }

    public class MaterialItem
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal StockTotal { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}