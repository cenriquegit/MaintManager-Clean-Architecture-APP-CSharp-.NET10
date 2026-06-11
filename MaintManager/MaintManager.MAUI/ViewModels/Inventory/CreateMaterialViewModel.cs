using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class CreateMaterialViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int? _editMateid;

    public CreateMaterialViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Nuevo Material";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mateid", out var id) && id is string idStr && int.TryParse(idStr, out var mateid))
        {
            _editMateid = mateid;
            Title = "Editar Material";
            LoadMaterialCommand.Execute(mateid);
        }
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
    private async Task LoadMaterial(int mateid)
    {
        await LoadCategories();
        var raw = await _apiService.GetAsync<ApiResponse<MaterialDetail>>($"api/v1/inventory/materials/{mateid}");
        if (raw?.Success == true && raw.Data is not null)
        {
            MaterialName = raw.Data.Name;
            UnitOfMeasure = raw.Data.UnitOfMeasure;
            StockMinimum = raw.Data.StockMinimum.ToString();
            Description = raw.Data.Description ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(c => c.Macaid == raw.Data.Macaid);
        }
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

            var payload = new
            {
                Macaid = SelectedCategory.Macaid,
                Name = MaterialName.Trim(),
                UnitOfMeasure = string.IsNullOrWhiteSpace(UnitOfMeasure) ? "Unidad" : UnitOfMeasure.Trim(),
                StockMinimum = (decimal)min,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
            };

            if (_editMateid.HasValue)
                await _apiService.PutAsync<object>($"api/v1/inventory/materials/{_editMateid}", payload);
            else
                await _apiService.PostAsync<object>("api/v1/inventory/materials", payload);

            await Shell.Current.DisplayAlert(_editMateid.HasValue ? "Material actualizado" : "Material creado",
                $"{MaterialName} se {(_editMateid.HasValue ? "actualizó" : "agregó")} al inventario.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    public class MaterialDetail
    {
        public int Mateid { get; set; }
        public short Macaid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal StockMinimum { get; set; }
        public string? Description { get; set; }
    }

    public class CategoryOption
    {
        public short Macaid { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }
}
