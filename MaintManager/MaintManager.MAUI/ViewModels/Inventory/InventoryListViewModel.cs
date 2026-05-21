using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Models;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
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
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyLowStock;

    [ObservableProperty]
    private ObservableCollection<MaterialItem> _materials = new();

    [ObservableProperty]
    private int? _lowStockCount;

    [ObservableProperty]
    private bool _isAdmin = true; // Ajustar según rol real

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ShowOnlyLowStock ? ApiRoutes.Inventory.GetLowStock : ApiRoutes.Inventory.GetMaterials;
            var response = await _apiService.GetAsync<ApiResponse<List<MaterialItem>>>(endpoint);
            if (response?.Success == true)
            {
                var items = response.Data ?? new List<MaterialItem>();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    items = items.Where(m =>
                        m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        m.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                Materials = new ObservableCollection<MaterialItem>(items);
                LowStockCount = Materials.Count(m => m.IsBelowMinimum);
                IsEmpty = Materials.Count == 0;
            }
            else
            {
                throw new Exception(response?.Message ?? "Error al cargar inventario");
            }
        });
    }

    partial void OnShowOnlyLowStockChanged(bool value) => LoadCommand.Execute(null);
    partial void OnSearchTextChanged(string value) => LoadCommand.Execute(null);

    [RelayCommand]
    private async Task AddLot()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Task.Delay(200);

        await Shell.Current.GoToAsync("///Inventory/CreateLot");
    }

    [RelayCommand]
    private async Task CreateMaterial()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Task.Delay(200);

        await Shell.Current.GoToAsync("///Inventory/CreateMaterial");
    }

    [RelayCommand]
    private async Task ViewLots(MaterialItem material)
    {
        if (material is null) return;
        Shell.Current.FlyoutIsPresented = false;
        await Task.Delay(200);

        var parameters = new Dictionary<string, object>
        {
            { "mateid", material.Mateid },
            { "name", material.Name }
        };
        await Shell.Current.GoToAsync("///Inventory/LotList", parameters);
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}