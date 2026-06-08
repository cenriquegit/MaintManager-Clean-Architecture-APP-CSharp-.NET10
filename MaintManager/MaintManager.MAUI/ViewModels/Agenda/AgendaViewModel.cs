using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Agenda;

public partial class AgendaViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public AgendaViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Agenda de Servicios";
    }

    [ObservableProperty]
    private ObservableCollection<AgendaItem> _overdue = new();

    [ObservableProperty]
    private ObservableCollection<AgendaItem> _upcoming = new();

    [ObservableProperty]
    private ObservableCollection<AgendaItem> _inService = new();

    [ObservableProperty]
    private ObservableCollection<AgendaItem> _ok = new();

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<AgendaData>>("api/v1/agenda");
            if (response?.Success == true && response.Data is not null)
            {
                Overdue = new ObservableCollection<AgendaItem>(response.Data.Overdue ?? new());
                Upcoming = new ObservableCollection<AgendaItem>(response.Data.Upcoming ?? new());
                InService = new ObservableCollection<AgendaItem>(response.Data.InService ?? new());
                Ok = new ObservableCollection<AgendaItem>(response.Data.Ok ?? new());
                IsEmpty = Overdue.Count + Upcoming.Count + InService.Count + Ok.Count == 0;
            }
            else
            {
                HasError = true;
                ErrorMessage = "No se pudieron cargar los datos de la agenda.";
            }
        });
    }

    [RelayCommand]
    private async Task OpenItem(AgendaItem? item)
    {
        if (item is null) return;
        if (item.InService)
            await Shell.Current.GoToAsync($"///Maintenances/Detail?mainid={item.OrderId}");
        else
            await Shell.Current.GoToAsync($"///Maintenances/Create?prcoid={item.Prcoid}&type=scheduled");
    }

    [RelayCommand]
    private async Task CreateNew()
    {
        await Shell.Current.GoToAsync("///Maintenances/Create");
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public class AgendaData
    {
        public List<AgendaItem>? Overdue { get; set; }
        public List<AgendaItem>? Upcoming { get; set; }
        public List<AgendaItem>? InService { get; set; }
        public List<AgendaItem>? Ok { get; set; }
    }

    public class AgendaItem
    {
        public int MvId { get; set; }
        public int Prcoid { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public short? Year { get; set; }
        public int OrderId { get; set; }
        public string? ServiceType { get; set; }
        public int? CurrentKm { get; set; }
        public int? NextKm { get; set; }
        public int? KmDiff { get; set; }

        public bool InService => OrderId > 0;
        public string StatusLabel => InService ? ServiceType ?? "En Servicio" :
            (KmDiff.HasValue && KmDiff.Value < 0 ? $"Vencido — +{Math.Abs(KmDiff.Value):N0} km extra" :
            KmDiff.HasValue ? $"Faltan {KmDiff.Value:N0} km" : "Sin datos");
        public string KmLine => CurrentKm.HasValue && NextKm.HasValue
            ? $"{CurrentKm:N0} km · Próximo: {NextKm:N0} km"
            : "Kilometraje no disponible";
        public string SectionLabel => InService ? "🟢 EN SERVICIO" :
            (KmDiff.HasValue && KmDiff.Value < 0 ? "🔴 CRÍTICO — KM vencido" :
            "Al día");
    }
}
