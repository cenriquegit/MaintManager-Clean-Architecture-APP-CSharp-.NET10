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

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "No Resueltas";

    public List<string> FilterOptions { get; } = new() { "No Resueltas", "Leídas", "No Leídas", "Resueltas", "Todas" };

    partial void OnSelectedFilterChanged(string value) => SearchCommand.Execute(null);

    [RelayCommand]
    private void Search() => LoadCommand.Execute(null);

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            IsAdmin = _authService.IsAdmin();

            var filter = SelectedFilter switch
            {
                "Leídas" => "read",
                "No Leídas" => "unread",
                "Resueltas" => "resolved",
                "Todas" => "all",
                _ => "unresolved"
            };

            var raw = await _apiService.GetAsync<ApiResponse<List<AlertItem>>>($"{ApiRoutes.Alerts.GetUnresolved}?filter={filter}");
            if (raw?.Success == true && raw.Data is not null)
            {
                var alerts = raw.Data;
                if (!string.IsNullOrWhiteSpace(SearchText))
                    alerts = alerts.Where(a => a.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                Alerts = new ObservableCollection<AlertItem>(alerts);
            }
            IsEmpty = Alerts.Count == 0;
        });
    }

    [RelayCommand]
    private async Task MarkRead(AlertItem alert)
    {
        if (alert.IsRead) return;
        await _apiService.PutAsync<object>($"{ApiRoutes.Alerts.MarkRead.Replace("{id}", alert.Alloid.ToString())}");
        alert.IsRead = true;
        alert.IsResolved = false;
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
