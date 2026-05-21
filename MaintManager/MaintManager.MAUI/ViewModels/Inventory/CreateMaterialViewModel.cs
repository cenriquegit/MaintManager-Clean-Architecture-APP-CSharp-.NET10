using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class CreateMaterialViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public CreateMaterialViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Nuevo Material";
    }

    [ObservableProperty]
    private ObservableCollection<CategoryOption> _categories = new();

    [ObservableProperty]
    private CategoryOption? _selectedCategory;

    [ObservableProperty]
    private string _materialName = string.Empty;

    [ObservableProperty]
    private string _unitOfMeasure = string.Empty;

    [ObservableProperty]
    private string _stockMinimum = "0";

    [ObservableProperty]
    private string _description = string.Empty;

    [RelayCommand]
    private async Task LoadCategories()
    {
        try
        {
            var response = await _apiService.GetAsync<ApiResponse<List<CategoryOption>>>("api/v1/inventory/categories");
            if (response?.Success == true && response.Data is not null)
                Categories = new ObservableCollection<CategoryOption>(response.Data);
        }
        catch { }
    }

    [RelayCommand]
    private async Task Save()
    {
        await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(MaterialName))
            {
                ErrorMessage = "El nombre del material es obligatorio.";
                HasError = true;
                return;
            }
            if (SelectedCategory is null)
            {
                ErrorMessage = "Selecciona una categoría.";
                HasError = true;
                return;
            }
            if (!int.TryParse(StockMinimum, out var min) || min < 0)
            {
                ErrorMessage = "El stock mínimo debe ser un número válido.";
                HasError = true;
                return;
            }

            await _apiService.PostAsync<object>("api/v1/inventory/materials", new
            {
                Macaid = SelectedCategory.Macaid,
                Name = MaterialName.Trim(),
                UnitOfMeasure = string.IsNullOrWhiteSpace(UnitOfMeasure) ? "Unidad" : UnitOfMeasure.Trim(),
                StockMinimum = (decimal)min,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
            });

            await Shell.Current.DisplayAlert("Material creado",
                $"{MaterialName} se agregó al inventario.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    public class CategoryOption
    {
        public short Macaid { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }
}
