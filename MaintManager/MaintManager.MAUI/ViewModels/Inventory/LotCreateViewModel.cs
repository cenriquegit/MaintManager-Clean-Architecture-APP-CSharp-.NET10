using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Models;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MaintManager.MAUI.ViewModels.Inventory;

public partial class LotCreateViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LotCreateViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Ingresar Lote";
    }

    private sealed class MaterialListRaw
    {
        public int Mateid { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? UnitOfMeasure { get; init; }
    }

    private sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    [ObservableProperty]
    private bool _formReady;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationError;

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

    public DateTime ExpirationMinDate => DateTime.Today;

    [ObservableProperty]
    private bool _isSaving;

    [RelayCommand]
    private async Task LoadMaterials()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<MaterialListRaw>>>(ApiRoutes.Inventory.GetMaterials);
            if (response?.Success == true && response.Data is not null)
            {
                Materials.Clear();
                foreach (var m in response.Data)
                    Materials.Add(new MaterialOption { Mateid = m.Mateid, Name = m.Name, UnitOfMeasure = m.UnitOfMeasure ?? "" });
                FormReady = true;
            }
            else
            {
                HasError = true;
                ErrorMessage = "Error al cargar materiales. Intenta nuevamente.";
            }
        });
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedMaterial == null)
        {
            ValidationMessage = "Seleccione un material.";
            HasValidationError = true;
            return;
        }

        if (Quantity <= 0)
        {
            ValidationMessage = "La cantidad debe ser mayor a cero.";
            HasValidationError = true;
            return;
        }

        IsSaving = true;
        HasValidationError = false;
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
        catch (HttpRequestException ex)
        {
            ValidationMessage = ex.Message;
            HasValidationError = true;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error al registrar el lote: {ex.Message}";
            HasValidationError = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Inicializar al crear la página
    public void OnAppearing()
    {
        if (Materials.Count == 0)
            LoadMaterialsCommand.Execute(null);
    }
}