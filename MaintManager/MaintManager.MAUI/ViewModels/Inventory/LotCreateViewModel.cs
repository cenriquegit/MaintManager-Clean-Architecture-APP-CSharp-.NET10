using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class LotCreateViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public LotCreateViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Ingresar Lote";
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isSuccess = true; // Iniciar con formulario visible

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MaterialOption> _materials = new();

    [ObservableProperty]
    private MaterialOption? _selectedMaterial;

    [ObservableProperty]
    private int _mateid;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private decimal _unitCost;

    [ObservableProperty]
    private string _supplierLotNumber = string.Empty;

    [ObservableProperty]
    private bool _hasExpiration;

    [ObservableProperty]
    private DateTime _expirationDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private bool _isSaving;

    [RelayCommand]
    private async Task LoadMaterials()
    {
        IsLoading = true;
        HasError = false;
        try
        {
            // Simulación: cargar materiales desde API
            Materials.Clear();
            Materials.Add(new MaterialOption { Mateid = 1, Name = "Aceite 15W40", UnitOfMeasure = "Litro" });
            Materials.Add(new MaterialOption { Mateid = 2, Name = "Filtro de aceite", UnitOfMeasure = "Unidad" });
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedMaterial == null)
        {
            ValidationMessage = "Seleccione un material.";
            return;
        }

        IsSaving = true;
        try
        {
            var request = new
            {
                Mateid = SelectedMaterial.Mateid,
                Quantity,
                UnitCost,
                ExpirationDate = HasExpiration ? ExpirationDate.ToString("yyyy-MM-dd") : null,
                SupplierLotNumber
            };
            await _apiService.PostAsync<object>($"api/v1/inventory/materials/{SelectedMaterial.Mateid}/lots", request);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public class MaterialOption
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    // Inicializar al crear la página
    public void OnAppearing()
    {
        if (Materials.Count == 0)
            LoadMaterialsCommand.Execute(null);
    }
}