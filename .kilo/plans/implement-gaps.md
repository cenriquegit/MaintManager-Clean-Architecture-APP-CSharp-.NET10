# Implementación de gaps: Descarte de lotes + Instalar componentes

## Feature 1: Descarte de lotes (RF-11)

### Archivos a modificar:

#### `Shared/Constants/ApiRoutes.cs`
Agregar:
```csharp
public const string DiscardLot = $"{Base}/lots/{{lotId}}/discard";
```

#### `MAUI/ViewModels/Inventory/LotListViewModel.cs`
Agregar `DiscardLotCommand`:
```csharp
[RelayCommand]
private async Task DiscardLot(LotItem lot)
{
    var confirm = await Shell.Current.DisplayAlert("Descartar lote",
        $"¿Estás seguro de descartar \"{lot.LotNumberDisplay}\"?\n" +
        $"Cantidad actual: {lot.CurrentQuantity:N1}\n" +
        $"Se marcará como descartado y no estará disponible para consumo.",
        "Sí, descartar", "Cancelar");
    if (!confirm) return;

    var reason = await Shell.Current.DisplayPromptAsync(
        "Motivo del descarte",
        "Indica por qué se descarta este lote:",
        "Confirmar", "Cancelar",
        placeholder: "ej: Vencido, dañado, contaminado...");
    if (string.IsNullOrWhiteSpace(reason)) return;

    await ExecuteAsync(async () =>
    {
        var endpoint = ApiRoutes.Inventory.DiscardLot.Replace("{lotId}", lot.Maloid.ToString());
        await _apiService.PostAsync<object>(endpoint, new
        {
            Quantity = lot.CurrentQuantity,
            Reason = reason,
            Note = (string?)null
        });
        await Load();
    });
}
```

#### `MAUI/Views/Inventory/LotListPage.xaml`
En cada tarjeta de lote, agregar botón "Descartar" (solo visible para Admin y si el lote está activo):
```xml
<!-- Botón Descartar -->
<Button Text="Descartar"
        Command="{Binding Source={RelativeSource AncestorType={x:Type vm:LotListViewModel}}, Path=DiscardLotCommand}"
        CommandParameter="{Binding .}"
        IsVisible="{Binding LotStatus, Converter={StaticResource IsActiveLotConverter}}"
        Style="{StaticResource SecondaryButton}"
        FontSize="12" HeightRequest="34" Padding="10,0"
        BackgroundColor="{StaticResource ColorError}" TextColor="White"/>
```

Agregar converter `IsActiveLotConverter` que retorna true solo si `LotStatus == "activo"`.

#### `MauiProgram.cs`
Registrar converter:
```csharp
builder.Services.AddSingleton<IsActiveLotConverter>();
```

O crear converter inline en App.xaml.

---

## Feature 2: Instalar componente durante mantenimiento (RF-12)

### Archivos a modificar:

#### `API/Controllers/MaintenancesController.cs`
Agregar endpoint para catálogo de acciones:
```csharp
/// <summary>Obtener catálogo de acciones (para instalar componentes).</summary>
[HttpGet("actions/catalog")]
[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ActionCatalogItem>>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetActionCatalog(CancellationToken ct)
{
    var actions = await _context.ActionCatalogs
        .Where(a => a.Status)
        .OrderBy(a => a.Category).ThenBy(a => a.Name)
        .Select(a => new ActionCatalogItem(a.Acatid, a.Name, a.Category))
        .ToListAsync(ct);
    return Ok(ApiResponse<IReadOnlyList<ActionCatalogItem>>.Ok(actions));
}

// DTO temporal (o mover a Application/DTOs)
public sealed record ActionCatalogItem(int Acatid, string Name, string? Category);
```

#### `MAUI/ViewModels/Maintenances/MaintenanceDetailViewModel.cs`
Agregar:
```csharp
[ObservableProperty]
private ObservableCollection<ActionCatalogOption> _componentActions = new();

[ObservableProperty]
private ActionCatalogOption? _selectedComponentAction;

[RelayCommand]
private async Task InstallComponent()
{
    if (SelectedComponentAction is null) return;
    await ExecuteAsync(async () =>
    {
        var endpoint = ApiRoutes.Maintenances.InstallComponent.Replace("{id}", _mainid.ToString());
        await _apiService.PostAsync<object>(endpoint, new
        {
            ActionCatalogId = SelectedComponentAction.Acatid,
            LotId = (int?)null,
            UsefulLifeDays = (int?)null
        });
        SelectedComponentAction = null;
        await Load();
    });
}

// Cargar acciones disponibles
private async Task LoadComponentActionsAsync()
{
    try
    {
        var raw = await _apiService.GetAsync<ApiResponse<List<ActionCatalogOption>>>(
            ApiRoutes.Maintenances.ActionCatalog);
        if (raw?.Success == true && raw.Data is not null)
            ComponentActions = new(raw.Data);
    }
    catch { }
}

public class ActionCatalogOption
{
    public int Acatid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public override string ToString() => $"{Name}";
}
```

Llamar `LoadComponentActionsAsync()` en el `Load()` después de `LoadMaterialsAsync()`.

#### `MAUI/Views/Maintenances/MaintenanceDetailPage.xaml`
Agregar sección "Instalar Componente" entre la sección de componentes instalados y la sección de consumo:
```xml
<!-- Instalar Componente -->
<Border StrokeShape="RoundRectangle 12"
        Stroke="{StaticResource ColorBorderLight}"
        StrokeThickness="1" Padding="16"
        BackgroundColor="{StaticResource ColorSurface}">
    <VerticalStackLayout Spacing="10">
        <Label Text="Instalar Componente" FontSize="16" FontAttributes="Bold"/>
        <BoxView HeightRequest="1" BackgroundColor="{StaticResource ColorBorderLight}"/>
        
        <Label Text="Componente:" Style="{StaticResource CaptionText}"/>
        <Border StrokeShape="RoundRectangle 8"
                Stroke="{StaticResource ColorBorder}"
                StrokeThickness="1" Padding="4,0">
            <Picker ItemsSource="{Binding ComponentActions}"
                    SelectedItem="{Binding SelectedComponentAction}"
                    ItemDisplayBinding="{Binding .}"
                    Title="Seleccionar componente..."
                    Style="{StaticResource FormPicker}"/>
        </Border>

        <Button Text="Instalar"
                Command="{Binding InstallComponentCommand}"
                Style="{StaticResource AccentButton}"
                HorizontalOptions="Fill"/>
    </VerticalStackLayout>
</Border>
```

#### `Shared/Constants/ApiRoutes.cs`
Agregar (ya existe `InstallComponent`).

---

## Orden de ejecución

```bash
# 1. Agregar rutas a ApiRoutes.cs
# 2. Agregar endpoint ActionCatalog en MaintenancesController.cs
# 3. Modificar LotListViewModel.cs y LotListPage.xaml (descarte)
# 4. Modificar MaintenanceDetailViewModel.cs y MaintenanceDetailPage.xaml (componentes)
# 5. Recompilar API + MAUI
# 6. Instalar APK y probar
```
