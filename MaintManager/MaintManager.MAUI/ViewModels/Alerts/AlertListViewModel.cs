using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Models;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Alerts;

public partial class AlertListViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    public AlertListViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Alertas";
    }

    [ObservableProperty]
    private ObservableCollection<AlertItem> _alerts = new();

    [ObservableProperty]
    private bool _isAdmin;

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            IsAdmin = _authService.IsAdmin();
            var response = await _apiService.GetAsync<ApiResponse<List<AlertItem>>>(ApiRoutes.Alerts.GetUnresolved);
            if (response?.Success == true)
            {
                Alerts = new ObservableCollection<AlertItem>(response.Data ?? new List<AlertItem>());
                IsEmpty = Alerts.Count == 0;
            }
            else
            {
                throw new Exception(response?.Message ?? "Error al cargar alertas");
            }
        });
    }

    [RelayCommand]
    private async Task MarkRead(AlertItem alert)
    {
        if (alert.IsRead) return;
        await _apiService.PutAsync<object>($"{ApiRoutes.Alerts.MarkRead.Replace("{id}", alert.Alloid.ToString())}");
        alert.IsRead = true;
    }

    [RelayCommand]
    private async Task Resolve(AlertItem alert)
    {
        if (alert.IsResolved) return;
        await _apiService.PutAsync<object>($"{ApiRoutes.Alerts.Resolve.Replace("{id}", alert.Alloid.ToString())}");
        Alerts.Remove(alert);
        IsEmpty = Alerts.Count == 0;
    }

    [RelayCommand]
    private async Task CheckAlerts()
    {
        await ExecuteAsync(async () =>
        {
            await _apiService.PostAsync<object>(ApiRoutes.Alerts.Check);
            IsEmpty = false;
            await Load();
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}