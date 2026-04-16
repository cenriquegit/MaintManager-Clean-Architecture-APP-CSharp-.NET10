using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<AlertItem>>>("api/v1/alerts");
            if (response?.Success == true && response.Data != null)
            {
                Alerts = new ObservableCollection<AlertItem>(response.Data);
            }
        });
    }

    public class AlertItem
    {
        public int Alloid { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}