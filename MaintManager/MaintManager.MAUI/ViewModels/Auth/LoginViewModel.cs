using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;

namespace MaintManager.MAUI.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ApiService _apiService;

    public LoginViewModel(AuthService authService, ApiService apiService)
    {
        _authService = authService;
        _apiService = apiService;
        Title = "Iniciar Sesión";
        ApiUrl = Preferences.Get("api_url", ApiService.DefaultBaseUrl);
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _showPassword;

    [ObservableProperty]
    private bool _showUrlConfig;

    [ObservableProperty]
    private string _apiUrl = string.Empty;

    [RelayCommand]
    private void ToggleUrlConfig()
    {
        ShowUrlConfig = !ShowUrlConfig;
    }

    [RelayCommand]
    private void SaveUrlConfig()
    {
        if (!string.IsNullOrWhiteSpace(ApiUrl))
        {
            Preferences.Set("api_url", ApiUrl);
            _apiService.ApplySavedBaseUrl();
            ShowUrlConfig = false;
        }
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Usuario y contraseña requeridos";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//Dashboard");
            }
            else
            {
                ErrorMessage = "Credenciales inválidas";
            }
        });
    }
}