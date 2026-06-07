using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.VehicleConfig;

public partial class VehicleConfigViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int? _directMvId;
    private int? _directVehicleId;

    public VehicleConfigViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Config. Vehículos";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mvId", out var mv) && mv is string mvStr && int.TryParse(mvStr, out var mvId))
            _directMvId = mvId;
        else if (query.TryGetValue("vehicleId", out var vid) && vid is string vidStr && int.TryParse(vidStr, out var vehicleId))
            _directVehicleId = vehicleId;
    }

    [ObservableProperty]
    private ObservableCollection<VehicleOption> _vehicles = new();

    [ObservableProperty]
    private VehicleOption? _selectedVehicle;

    partial void OnSelectedVehicleChanged(VehicleOption? value)
    {
        if (value is not null)
            LoadConfigCommand.Execute(null);
    }

    [ObservableProperty]
    private ObservableCollection<ActionOption> _availableActions = new();

    [ObservableProperty]
    private ActionOption? _selectedAction;

    [ObservableProperty]
    private ObservableCollection<ActionOption> _allowedActions = new();

    [ObservableProperty]
    private ObservableCollection<MaterialOptionItem> _availableMaterials = new();

    [ObservableProperty]
    private MaterialOptionItem? _selectedMaterial;

    [ObservableProperty]
    private ObservableCollection<MaterialOptionItem> _allowedMaterials = new();

    [ObservableProperty]
    private ObservableCollection<ActionOption> _availableComponents = new();

    [ObservableProperty]
    private ActionOption? _selectedComponent;

    [ObservableProperty]
    private ObservableCollection<ActionOption> _allowedComponents = new();

    // ── Load ───────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadVehicles()
    {
        List<VehicleOption>? loaded = null;

        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<ManagedVehicleItem>>>("api/v1/vehicles/managed");
            if (response?.Success == true && response.Data is not null)
            {
                loaded = response.Data.Select(v => new VehicleOption
                {
                    Prcoid = v.Prcoid ?? 0,
                    MvId = v.MvId,
                    LicensePlate = v.LicensePlate,
                    VehicleName = v.VehicleName,
                    Source = v.Source,
                }).ToList();
                Vehicles = new ObservableCollection<VehicleOption>(loaded);
            }
            else
            {
                HasError = true;
                ErrorMessage = "Error al cargar vehículos.";
            }
        });

        if (loaded is null || loaded.Count == 0 || HasError) return;

        VehicleOption? toSelect = null;
        if (_directMvId.HasValue || _directVehicleId.HasValue)
            toSelect = loaded.FirstOrDefault(v =>
                (_directMvId.HasValue && v.MvId == _directMvId.Value) ||
                (_directVehicleId.HasValue && v.Prcoid == _directVehicleId.Value));
        else if (SelectedVehicle is null)
            toSelect = loaded[0];

        if (toSelect is not null)
            SelectedVehicle = toSelect;
    }

    [RelayCommand]
    private async Task LoadConfig()
    {
        if (SelectedVehicle is null) return;
        var prcoid = SelectedVehicle.Prcoid;
        var mvId = SelectedVehicle.MvId;
        var isManaged = mvId > 0;
        var configUrl = isManaged
            ? $"{ApiRoutes.Vehicles.Base}/{prcoid}/config?mvId={mvId}"
            : $"{ApiRoutes.Vehicles.Base}/{prcoid}/config";

        await ExecuteAsync(async () =>
        {
            var configTask = _apiService.GetAsync<ApiResponse<VehicleConfigData>>(configUrl);
            var actionsTask = _apiService.GetAsync<ApiResponse<List<ActionOption>>>(ApiRoutes.Maintenances.ActionCatalog);
            var materialsTask = _apiService.GetAsync<ApiResponse<List<MaterialOptionItem>>>(ApiRoutes.Inventory.GetMaterials);

            await Task.WhenAll(configTask, actionsTask, materialsTask);
            var config = await configTask;
            var allActions = await actionsTask;
            var allMaterials = await materialsTask;

            var allowedActionIds = new HashSet<int>();
            var allowedMaterialIds = new HashSet<int>();
            var allowedComponentIds = new HashSet<int>();

            if (config?.Success == true && config.Data is not null)
            {
                AllowedActions = new ObservableCollection<ActionOption>(config.Data.AllowedActions ?? new List<ActionOption>());
                AllowedMaterials = new ObservableCollection<MaterialOptionItem>(config.Data.AllowedMaterials ?? new List<MaterialOptionItem>());
                AllowedComponents = new ObservableCollection<ActionOption>(config.Data.AllowedComponents ?? new List<ActionOption>());
                allowedActionIds = config.Data.AllowedActions?.Select(a => a.Acatid).ToHashSet() ?? new();
                allowedMaterialIds = config.Data.AllowedMaterials?.Select(m => m.Mateid).ToHashSet() ?? new();
                allowedComponentIds = config.Data.AllowedComponents?.Select(c => c.Acatid).ToHashSet() ?? new();
            }
            else if (config?.Success == false)
            {
                AllowedActions.Clear();
                AllowedMaterials.Clear();
                AllowedComponents.Clear();
            }

            if (allActions?.Success == true && allActions.Data is not null)
            {
                AvailableActions = new ObservableCollection<ActionOption>(
                    allActions.Data.Where(a => (a.Category == null || !a.Category.Contains("Componente")) && !allowedActionIds.Contains(a.Acatid)));
                AvailableComponents = new ObservableCollection<ActionOption>(
                    allActions.Data.Where(a => a.Category is not null && a.Category.Contains("Componente") && !allowedComponentIds.Contains(a.Acatid)));
            }

            if (allMaterials?.Success == true && allMaterials.Data is not null)
            {
                AvailableMaterials = new ObservableCollection<MaterialOptionItem>(
                    allMaterials.Data.Where(m => !allowedMaterialIds.Contains(m.Mateid)));
            }

            IsEmpty = false;
        });
    }

    private string ConfigUrl(string suffix)
    {
        if (SelectedVehicle is null) return string.Empty;
        var prcoid = SelectedVehicle.Prcoid;
        var mvId = SelectedVehicle.MvId;
        var isManaged = mvId > 0;
        var baseUrl = $"{ApiRoutes.Vehicles.Base}/{prcoid}/config{suffix}";
        return isManaged ? $"{baseUrl}?mvId={mvId}" : baseUrl;
    }

    // ── Add/Remove Actions ─────────────────────────────────────

    [RelayCommand]
    private async Task AddAction()
    {
        if (SelectedVehicle is null || SelectedAction is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.PostAsync<object>(ConfigUrl("/actions"), new { acatid = SelectedAction.Acatid }); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    [RelayCommand]
    private async Task RemoveAction(ActionOption? action)
    {
        if (SelectedVehicle is null || action is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.DeleteAsync(ConfigUrl($"/actions/{action.Acatid}")); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    [RelayCommand]
    private async Task AddMaterial()
    {
        if (SelectedVehicle is null || SelectedMaterial is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.PostAsync<object>(ConfigUrl("/materials"), new { mateid = SelectedMaterial.Mateid }); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    [RelayCommand]
    private async Task RemoveMaterial(MaterialOptionItem? material)
    {
        if (SelectedVehicle is null || material is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.DeleteAsync(ConfigUrl($"/materials/{material.Mateid}")); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    [RelayCommand]
    private async Task AddComponent()
    {
        if (SelectedVehicle is null || SelectedComponent is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.PostAsync<object>(ConfigUrl("/components"), new { acatid = SelectedComponent.Acatid }); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    [RelayCommand]
    private async Task RemoveComponent(ActionOption? component)
    {
        if (SelectedVehicle is null || component is null) return;
        if (IsBusy) return;
        try { IsBusy = true; await _apiService.DeleteAsync(ConfigUrl($"/components/{component.Acatid}")); }
        catch (Exception ex) { HasError = true; ErrorMessage = ex.Message; }
        finally { IsBusy = false; await LoadConfig(); }
    }

    // ── Navigate ───────────────────────────────────────────────

    [RelayCommand]
    private async Task BackToVehicles()
    {
        await Shell.Current.GoToAsync("///Vehicles");
    }

    [RelayCommand]
    private async Task CreateAction()
    {
        if (SelectedVehicle is null) return;
        try { await Shell.Current.GoToAsync($"///ConfigVehicle/CreateAction?prcoid={SelectedVehicle.Prcoid}&mvId={SelectedVehicle.MvId}"); }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task CreateMaterial()
    {
        try { await Shell.Current.GoToAsync("///ConfigVehicle/CreateMaterial"); }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task CreateComponent()
    {
        if (SelectedVehicle is null) return;
        try { await Shell.Current.GoToAsync($"///ConfigVehicle/CreateComponent?prcoid={SelectedVehicle.Prcoid}&mvId={SelectedVehicle.MvId}"); }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error: {ex.Message}"; }
    }

    // ── Nested types ───────────────────────────────────────────

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public class ManagedVehicleItem
    {
        public int MvId { get; set; }
        public int? Prcoid { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string Source { get; set; } = "managed";
    }

    public class VehicleOption
    {
        public int Prcoid { get; set; }
        public int MvId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string Source { get; set; } = "managed";
        public override string ToString() => $"{VehicleName} — {LicensePlate}";
    }

    public class ActionOption
    {
        public int Acatid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public override string ToString() => Name;
    }

    public class MaterialOptionItem
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UnitOfMeasure { get; set; }
        public override string ToString() => $"{Name} ({UnitOfMeasure})";
    }

    public class VehicleConfigData
    {
        public int Prcoid { get; set; }
        public List<ActionOption>? AllowedActions { get; set; }
        public List<MaterialOptionItem>? AllowedMaterials { get; set; }
        public List<ActionOption>? AllowedComponents { get; set; }
    }
}
