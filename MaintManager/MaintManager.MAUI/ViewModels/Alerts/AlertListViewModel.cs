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

    public AlertListViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Alertas";
    }

    [ObservableProperty]
    private ObservableCollection<AlertItem> _alerts = new();

    [ObservableProperty]
    private bool _isAdmin; // Se debe setear después del login según el rol

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
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
        if (alert.Resolved) return;
        await _apiService.PutAsync<object>($"{ApiRoutes.Alerts.Resolve.Replace("{id}", alert.Alloid.ToString())}");
        Alerts.Remove(alert);
        IsEmpty = Alerts.Count == 0;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}