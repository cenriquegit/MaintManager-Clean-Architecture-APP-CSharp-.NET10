using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;

namespace MaintManager.MAUI.ViewModels.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    public SettingsViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Configuración";

        ApiUrl = Preferences.Get("api_url", "http://10.0.2.2:5056");
        UserFullName = Preferences.Get("user_fullname", "Usuario");
        UserRole = Preferences.Get("user_role", "Técnico");
        IsAdmin = UserRole.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
    }

    [ObservableProperty]
    private string _apiUrl = string.Empty;

    [ObservableProperty]
    private string _userFullName = string.Empty;

    [ObservableProperty]
    private string _userRole = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [RelayCommand]
    private async Task SaveSettings()
    {
        await ExecuteAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(ApiUrl))
            {
                Preferences.Set("api_url", ApiUrl);
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
