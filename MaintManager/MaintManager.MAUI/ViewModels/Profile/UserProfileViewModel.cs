using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Profile;

public partial class UserProfileViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ApiService _apiService;

    public UserProfileViewModel(AuthService authService, ApiService apiService)
    {
        _authService = authService;
        _apiService = apiService;
        Title = "Mi Perfil";

        UserName = _authService.GetFullName() ?? "Usuario";
        UserUsername = _authService.GetUsername() ?? "-";
        UserRole = _authService.GetRole() ?? "Técnico";
        IsAdmin = _authService.IsAdmin();
    }

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _userUsername = string.Empty;

    [ObservableProperty]
    private string _userRole = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    // Create user fields
    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newFln = string.Empty;

    [ObservableProperty]
    private string _newMln = string.Empty;

    [ObservableProperty]
    private string _newEmail = string.Empty;

    [ObservableProperty]
    private string _newRole = "Mecánico";

    public ObservableCollection<string> RoleOptions { get; } =
    [
        "Administrador",
        "Mecánico",
        "Supervisor",
        "Analista",
    ];

    [RelayCommand]
    private async Task CreateUser()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
        {
            await Shell.Current.DisplayAlert("Error", "Usuario y contraseña son requeridos.", "Aceptar");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var request = new
            {
                username = NewUsername,
                password = NewPassword,
                name = NewName,
                fln = NewFln,
                mln = string.IsNullOrWhiteSpace(NewMln) ? null : NewMln,
                email = string.IsNullOrWhiteSpace(NewEmail) ? null : NewEmail,
                role = NewRole,
            };

            await _apiService.PostAsync<object>("api/v1/workers", request);
            await Shell.Current.DisplayAlert("Éxito", $"Usuario '{NewUsername}' creado correctamente.", "Aceptar");

            NewUsername = string.Empty;
            NewPassword = string.Empty;
            NewName = string.Empty;
            NewFln = string.Empty;
            NewMln = string.Empty;
            NewEmail = string.Empty;
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
