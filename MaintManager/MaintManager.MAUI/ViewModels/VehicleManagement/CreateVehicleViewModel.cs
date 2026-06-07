using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.IO;

namespace MaintManager.MAUI.ViewModels.VehicleManagement;

public partial class CreateVehicleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int? _editMvId;

    public CreateVehicleViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Nuevo Vehículo";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mvId", out var id) && id is string idStr && int.TryParse(idStr, out var mvId))
        {
            _editMvId = mvId;
            Title = "Editar Vehículo";
            LoadVehicleCommand.Execute(mvId);
        }
    }

    // ── Fields ───────────────────────────────────────────────

    [ObservableProperty]
    private string _licensePlate = string.Empty;

    [ObservableProperty]
    private string _vehicleName = string.Empty;

    [ObservableProperty]
    private string _brand = string.Empty;

    [ObservableProperty]
    private string _model = string.Empty;

    [ObservableProperty]
    private string _year = string.Empty;

    [ObservableProperty]
    private string _color = string.Empty;

    [ObservableProperty]
    private string _vin = string.Empty;

    [ObservableProperty]
    private string _engineNumber = string.Empty;

    [ObservableProperty]
    private string _saveButtonText = "Guardar Vehículo";

    // ── SUNARP ────────────────────────────────────────────────

    [ObservableProperty]
    private bool _sunarpFieldsLocked = true;

    [ObservableProperty]
    private string _lockToggleText = "🔒 Desbloquear";

    [ObservableProperty]
    private bool _showCaptcha;

    [ObservableProperty]
    private ImageSource? _captchaImageSource;

    [ObservableProperty]
    private string _captchaCode = string.Empty;

    [ObservableProperty]
    private string _sunarpMessage = string.Empty;

    [ObservableProperty]
    private bool _hasSunarpMessage;

    partial void OnSunarpFieldsLockedChanged(bool value)
    {
        LockToggleText = value ? "🔒 Desbloquear" : "🔓 Bloquear";
    }

    [RelayCommand]
    private async Task GetCaptcha()
    {
        if (string.IsNullOrWhiteSpace(LicensePlate)) return;
        await ExecuteAsync(async () =>
        {
            var url = $"api/v1/vehicles/managed/sunarp-captcha?plate={LicensePlate.ToUpper()}";
            var response = await _apiService.PostAsync<ApiResponse<SunarpCaptchaDto>>(url);
            if (response?.Success == true && response.Data?.Success == true && response.Data.CaptchaBase64 is not null)
            {
                try
                {
                    var bytes = Convert.FromBase64String(response.Data.CaptchaBase64);
                    CaptchaImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                catch { CaptchaImageSource = null; }
                ShowCaptcha = true;
                SunarpMessage = "Ingresa el código de la imagen.";
                HasSunarpMessage = true;
            }
            else
            {
                SunarpMessage = response?.Data?.Error ?? "No se pudo obtener el captcha. Intenta más tarde.";
                HasSunarpMessage = true;
            }
        });
    }

    [RelayCommand]
    private async Task ConsultSunarp()
    {
        if (string.IsNullOrWhiteSpace(CaptchaCode)) return;
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.PostAsync<ApiResponse<SunarpVehicleDto>>(
                "api/v1/vehicles/managed/sunarp-consult",
                new { plate = LicensePlate.ToUpper(), captchaCode = CaptchaCode });

            if (response?.Success == true && response.Data?.Success == true)
            {
                var d = response.Data;
                if (!string.IsNullOrWhiteSpace(d.Brand)) Brand = d.Brand;
                if (!string.IsNullOrWhiteSpace(d.Model)) Model = d.Model;
                if (d.Year.HasValue) Year = d.Year.ToString();
                if (!string.IsNullOrWhiteSpace(d.Color)) Color = d.Color;
                if (!string.IsNullOrWhiteSpace(d.Vin)) Vin = d.Vin;
                if (!string.IsNullOrWhiteSpace(d.EngineNumber)) EngineNumber = d.EngineNumber;
                ShowCaptcha = false;
                SunarpFieldsLocked = true;
                SunarpMessage = "¡Datos cargados desde SUNARP!";
                HasSunarpMessage = true;
            }
            else
            {
                SunarpMessage = response?.Data?.Error ?? "Error en la consulta SUNARP.";
                HasSunarpMessage = true;
            }
        });
    }

    [RelayCommand]
    private void ToggleSunarpLock()
    {
        SunarpFieldsLocked = !SunarpFieldsLocked;
    }

    // ── Load edit ─────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadVehicle(int mvId)
    {
        await ExecuteAsync(async () =>
        {
            var url = $"api/v1/vehicles/managed/{mvId}";
            var response = await _apiService.GetAsync<ApiResponse<ManagedVehicleItem>>(url);
            if (response?.Success == true && response.Data is not null)
            {
                var d = response.Data;
                LicensePlate = d.LicensePlate;
                VehicleName = d.VehicleName;
                Brand = d.Brand ?? string.Empty;
                Model = d.Model ?? string.Empty;
                Year = d.Year?.ToString() ?? string.Empty;
                Color = d.Color ?? string.Empty;
                Vin = d.Vin ?? string.Empty;
                EngineNumber = d.EngineNumber ?? string.Empty;
                SaveButtonText = "Actualizar Vehículo";
            }
            else
            {
                HasError = true;
                ErrorMessage = "No se pudo cargar la información del vehículo.";
            }
        });
    }

    // ── Save ──────────────────────────────────────────────────

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(LicensePlate) || string.IsNullOrWhiteSpace(VehicleName))
        {
            ErrorMessage = "Placa y nombre del vehículo son obligatorios.";
            HasError = true;
            return;
        }

        await ExecuteAsync(async () =>
        {
            var payload = new
            {
                licensePlate = LicensePlate.ToUpper(),
                vehicleName = VehicleName.Trim(),
                brand = string.IsNullOrWhiteSpace(Brand) ? null : Brand.Trim(),
                model = string.IsNullOrWhiteSpace(Model) ? null : Model.Trim(),
                year = short.TryParse(Year, out var y) ? y : (short?)null,
                color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim(),
                vin = string.IsNullOrWhiteSpace(Vin) ? null : Vin.Trim(),
                engineNumber = string.IsNullOrWhiteSpace(EngineNumber) ? null : EngineNumber.Trim()
            };

            if (_editMvId.HasValue)
            {
                await _apiService.PutAsync<object>($"api/v1/vehicles/managed/{_editMvId}", payload);
            }
            else
            {
                await _apiService.PostAsync<object>("api/v1/vehicles/managed", payload);
            }

            await Shell.Current.GoToAsync("..");
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public class SunarpCaptchaDto
    {
        public bool Success { get; set; }
        public string? CaptchaBase64 { get; set; }
        public string? Error { get; set; }
    }

    public class SunarpVehicleDto
    {
        public bool Success { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? Color { get; set; }
        public string? Vin { get; set; }
        public string? EngineNumber { get; set; }
        public string? Titular { get; set; }
        public string? Error { get; set; }
    }

    public class ManagedVehicleItem
    {
        public int MvId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public short? Year { get; set; }
        public string? Color { get; set; }
        public string? Vin { get; set; }
        public string? EngineNumber { get; set; }
    }
}
