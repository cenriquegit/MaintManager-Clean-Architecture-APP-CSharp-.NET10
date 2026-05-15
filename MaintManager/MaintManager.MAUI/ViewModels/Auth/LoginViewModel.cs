using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;

namespace MaintManager.MAUI.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Iniciar Sesión";
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _showPassword;

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    [RelayCommand]
    private async Task GoToSettings()
    {
        await Shell.Current.GoToAsync("//Settings");
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