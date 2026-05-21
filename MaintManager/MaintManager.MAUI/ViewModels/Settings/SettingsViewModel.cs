using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;

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

    [ObservableProperty]
    private string _intervalKm = "5000";

    [ObservableProperty]
    private string _alertKmThreshold = "800";

    [ObservableProperty]
    private bool _configLoaded;

    private const string SettingsPin = "1234";

    [RelayCommand]
    private async Task LoadConfig()
    {
        try
        {
            var response = await _apiService.GetAsync<ConfigResponse>("api/v1/config");
            if (response?.Data is not null)
            {
                foreach (var item in response.Data)
                {
                    if (item.Key == "intervalo_km")
                        IntervalKm = item.Value;
                    else if (item.Key == "alerta_km_umbral")
                        AlertKmThreshold = item.Value;
                }
                ConfigLoaded = true;
            }
        }
        catch { ConfigLoaded = false; }
    }

    [RelayCommand]
    private async Task SaveConfig()
    {
        await ExecuteAsync(async () =>
        {
            if (int.TryParse(IntervalKm, out var km) && km > 0)
                await _apiService.PutAsync<object>($"api/v1/config/intervalo_km", new { Value = km.ToString() });

            if (int.TryParse(AlertKmThreshold, out var alert) && alert > 0)
                await _apiService.PutAsync<object>($"api/v1/config/alerta_km_umbral", new { Value = alert.ToString() });

            await Shell.Current.DisplayAlert("Configuración", "Parámetros guardados correctamente.", "Aceptar");
        });
    }

    [RelayCommand]
    private void CheckPin()
    {
        if (PinInput == SettingsPin)
        {
            IsLocked = false;
            PinError = string.Empty;
            PinInput = string.Empty;
            LoadConfigCommand.Execute(null);
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
        if (!string.IsNullOrWhiteSpace(ApiUrl))
        {
            Preferences.Set("api_url", ApiUrl);
            _apiService.ApplySavedBaseUrl();
        }
        await Shell.Current.DisplayAlert("Configuración",
            "Cambios guardados correctamente.\nReinicia sesión para aplicar la nueva URL.", "Aceptar");
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

    private sealed class ConfigResponse
    {
        public bool Success { get; set; }
        public List<ConfigItem>? Data { get; set; }
    }

    private sealed class ConfigItem
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
