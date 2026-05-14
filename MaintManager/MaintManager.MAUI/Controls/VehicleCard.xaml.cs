using System.Windows.Input;

namespace MaintManager.MAUI.Controls;

public partial class VehicleCard : ContentView
{
    public static readonly BindableProperty LicensePlateProperty =
        BindableProperty.Create(nameof(LicensePlate), typeof(string), typeof(VehicleCard), string.Empty);

    public static readonly BindableProperty VehicleNameProperty =
        BindableProperty.Create(nameof(VehicleName), typeof(string), typeof(VehicleCard), string.Empty);

    public static readonly BindableProperty CurrentKmProperty =
        BindableProperty.Create(nameof(CurrentKm), typeof(string), typeof(VehicleCard), string.Empty);

    public static readonly BindableProperty NextMaintenanceKmProperty =
        BindableProperty.Create(nameof(NextMaintenanceKm), typeof(string), typeof(VehicleCard), string.Empty);

    public static readonly BindableProperty IsDueSoonProperty =
        BindableProperty.Create(nameof(IsDueSoon), typeof(bool), typeof(VehicleCard), false,
            propertyChanged: OnDueSoonChanged);

    public static readonly BindableProperty OnStartMaintenanceCommandProperty =
        BindableProperty.Create(nameof(OnStartMaintenanceCommand), typeof(ICommand), typeof(VehicleCard));

    public string LicensePlate
    {
        get => (string)GetValue(LicensePlateProperty);
        set => SetValue(LicensePlateProperty, value);
    }

    public string VehicleName
    {
        get => (string)GetValue(VehicleNameProperty);
        set => SetValue(VehicleNameProperty, value);
    }

    public string CurrentKm
    {
        get => (string)GetValue(CurrentKmProperty);
        set => SetValue(CurrentKmProperty, value);
    }

    public string NextMaintenanceKm
    {
        get => (string)GetValue(NextMaintenanceKmProperty);
        set => SetValue(NextMaintenanceKmProperty, value);
    }

    public bool IsDueSoon
    {
        get => (bool)GetValue(IsDueSoonProperty);
        set => SetValue(IsDueSoonProperty, value);
    }

    public ICommand? OnStartMaintenanceCommand
    {
        get => (ICommand?)GetValue(OnStartMaintenanceCommandProperty);
        set => SetValue(OnStartMaintenanceCommandProperty, value);
    }

    public VehicleCard()
    {
        InitializeComponent();
        UpdateStatusDot();
    }

    private static void OnDueSoonChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VehicleCard card)
            card.UpdateStatusDot();
    }

    private void UpdateStatusDot()
    {
        StatusDot.Color = IsDueSoon ? Color.FromArgb("#E53935") : Color.FromArgb("#2ECC71");
    }
}
