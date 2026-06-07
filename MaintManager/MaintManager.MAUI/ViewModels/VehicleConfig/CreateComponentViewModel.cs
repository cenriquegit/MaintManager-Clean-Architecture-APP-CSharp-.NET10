using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.MAUI.ViewModels.VehicleConfig;

public partial class CreateComponentViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _prcoid;

    public CreateComponentViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Nuevo Componente";
    }

    [ObservableProperty]
    private string _componentName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _usefulLifeDays = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("prcoid", out var id) && id is string idStr && int.TryParse(idStr, out var prcoid))
            _prcoid = prcoid;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(ComponentName))
        {
            ErrorMessage = "El nombre del componente es obligatorio.";
            HasError = true;
            return;
        }

        await ExecuteAsync(async () =>
        {
            var endpoint = $"{ApiRoutes.Vehicles.Base}/{_prcoid}/config/components/create";
            await _apiService.PostAsync<object>(endpoint, new
            {
                Name = ComponentName.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                UsefulLifeDays = string.IsNullOrWhiteSpace(UsefulLifeDays) ? null : (int?)int.Parse(UsefulLifeDays)
            });
            await Shell.Current.GoToAsync("..");
        });
    }
}
