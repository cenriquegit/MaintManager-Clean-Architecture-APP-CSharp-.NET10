using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;

namespace MaintManager.MAUI.ViewModels.VehicleConfig;

public partial class CreateActionViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _prcoid;

    public CreateActionViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Nueva Acción";
    }

    [ObservableProperty]
    private string _actionName = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "Acción";

    public List<string> CategoryOptions { get; } = ["Acción", "Componente"];

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
        if (string.IsNullOrWhiteSpace(ActionName))
        {
            ErrorMessage = "El nombre de la acción es obligatorio.";
            HasError = true;
            return;
        }

        await ExecuteAsync(async () =>
        {
            var endpoint = $"{ApiRoutes.Vehicles.Base}/{_prcoid}/config/actions/create";
            await _apiService.PostAsync<object>(endpoint, new
            {
                Name = ActionName.Trim(),
                Category = SelectedCategory,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                UsefulLifeDays = string.IsNullOrWhiteSpace(UsefulLifeDays) ? null : (int?)int.Parse(UsefulLifeDays)
            });
            await Shell.Current.GoToAsync("..");
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }
}
