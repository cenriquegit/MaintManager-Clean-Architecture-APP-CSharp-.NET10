# Plan: Botón "Historial" en cada card de vehículo

## Diagnóstico

### Estado actual

- `VehicleManagementPage.xaml`: cada card de vehículo tiene 2 botones: ✏️ (editar) y ⚙ (configuración)
- `VehicleHistoryPage` ya existe con ruta registrada `Maintenances/VehicleHistory`
- `VehicleHistoryViewModel` acepta query params `vehicleId` + `vehicleName`
- Al tocar un mantenimiento en el historial, navega a `//Maintenances/Detail?mainid={id}`
- El botón "Atrás" en la detail page regresa al historial automáticamente

### Lo que falta

Agregar un tercer botón 📋 (historial) en cada card que navegue al historial de ese vehículo.

## Cambios (3 archivos)

### 1. ViewModel: `VehicleManagementViewModel.cs`

Agregar comando `NavigateToHistory`:

```csharp
[RelayCommand]
private async Task NavigateToHistory(ManagedVehicleItem? vehicle)
{
    if (vehicle is null) return;
    try
    {
        var id = vehicle.Prcoid ?? vehicle.MvId;
        await Shell.Current.GoToAsync($"///Maintenances/VehicleHistory?vehicleId={id}&vehicleName={Uri.EscapeDataString(vehicle.Display)}");
    }
    catch (Exception ex) { HasError = true; ErrorMessage = $"Error al navegar: {ex.Message}"; }
}
```

### 2. Code-behind: `VehicleManagementPage.xaml.cs`

Agregar handler `OnHistoryTapped`:

```csharp
private void OnHistoryTapped(object? sender, TappedEventArgs e)
{
    if (sender is BindableObject bo && bo.BindingContext is ManagedVehicleItem item)
        _viewModel.NavigateToHistoryCommand.Execute(item);
}
```

### 3. XAML: `VehicleManagementPage.xaml`

Agregar botón 📋 en el `HorizontalStackLayout` de los botones (Grid.Column="1"):

```xml
<Border StrokeShape="RoundRectangle 14" HeightRequest="28" WidthRequest="28" StrokeThickness="0"
        BackgroundColor="{StaticResource ColorSuccessContainer}">
    <Border.GestureRecognizers><TapGestureRecognizer Tapped="OnHistoryTapped"/></Border.GestureRecognizers>
    <Label Text="📋" FontSize="12" HorizontalOptions="Center" VerticalOptions="Center"
           TextColor="{StaticResource ColorSuccess}"/>
</Border>
```

## Archivos afectados

| # | Archivo | Cambio |
|---|---|---|
| 1 | `ViewModels/VehicleManagement/VehicleManagementViewModel.cs` | Agregar `NavigateToHistoryCommand` |
| 2 | `Views/VehicleManagement/VehicleManagementPage.xaml.cs` | Agregar `OnHistoryTapped` handler |
| 3 | `Views/VehicleManagement/VehicleManagementPage.xaml:62-71` | Agregar botón 📋 |
