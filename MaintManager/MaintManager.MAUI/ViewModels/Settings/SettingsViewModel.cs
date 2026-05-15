using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;

namespace MaintManager.MAUI.ViewModels.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ApiService _apiService;

    public SettingsViewModel(AuthService authService, ApiService apiService)
    {
        _authService = authService;
        _apiService = apiService;
        Title = "Configuración";

        ApiUrl = Preferences.Get("api_url", ApiService.DefaultBaseUrl);
    }

    [ObservableProperty]
    private string _apiUrl = string.Empty;

    [ObservableProperty]
    private string _pinInput = string.Empty;

    [ObservableProperty]
    private string _pinError = string.Empty;

    [ObservableProperty]
    private bool _isLocked = true;

    private const string SettingsPin = "1234";

    [RelayCommand]
    private void CheckPin()
    {
        if (PinInput == SettingsPin)
        {
            IsLocked = false;
            PinError = string.Empty;
            PinInput = string.Empty;
        }
        else
        {
            PinError = "PIN incorrecto. Intenta nuevamente.";
            PinInput = string.Empty;
        }
    }

    [RelayCommand]
    private async Task GoHome()
    {
        await Shell.Current.GoToAsync("//Login");
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        await ExecuteAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(ApiUrl))
            {
                Preferences.Set("api_url", ApiUrl);
                _apiService.ApplySavedBaseUrl();
            }
            await Shell.Current.DisplayAlert("Configuración", "Cambios guardados correctamente.", "Aceptar");
        });
    }

    [RelayCommand]
    private async Task Logout()
    {
        var confirm = await Shell.Current.DisplayAlert("Cerrar sesión",
            "¿Estás seguro de que deseas cerrar sesión?", "Sí", "Cancelar");

        if (confirm)
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
