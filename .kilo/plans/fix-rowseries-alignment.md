# Plan: Fix RowSeries tooltip — valor falso en hover

## Diagnóstico confirmado

### Trazabilidad del dato real

```sql
-- BiReportService.cs:66-75
CASE
    WHEN vk.current_km > 0
    THEN round(total_material_cost / current_km, 4)
    ELSE 0
END AS "CostPerKm"
```

Ejemplo con VDG-361:
- Si `total_material_cost` = 15000 y `current_km` = 25000 → `CostPerKm = 0.6000`
- La barra se dibuja a ~$0.600 en el eje X → **correcto**
- El tooltip muestra $0.000 → **falso**

### Causa raíz

LiveCharts 2.1.0-dev-570: el sistema de tooltip de `RowSeries<double>` lee `ChartPoint.PrimaryValue` que no está correctamente poblado cuando se usa `double[]` sin mapeo explícito. El render de barras usa otra ruta interna que sí lee los valores correctamente.

### Por qué `ObservablePoint` rompió los gráficos

`RowSeries<ObservablePoint>` en esta versión dev tiene un bug de renderizado distinto — no dibuja las barras. El tooltip sí podría ser correcto, pero sin barras visibles el gráfico queda inservible.

## Plan

### Paso 1: `Mapping` explícito en `RowSeries<double>` (intento más ligero)

El property `Mapping` de `RowSeries<double>` acepta `Func<double, int, Coordinate>` y permite definir explícitamente cómo cada `double` + índice se convierte en coordenada del gráfico.

```csharp
new RowSeries<double>
{
    Values = values,
    Mapping = (value, index) => new Coordinate(value, index),
    // Coordinate.X = value  → barra horizontal (eje X)
    // Coordinate.Y = index → posición vertical (eje Y)
    ...
}
```

Si esto fuerza al tooltip a leer la coordenada correcta, problema resuelto con mínimo cambio.

### Paso 2: `TooltipLabelFormatter` (si Mapping no resuelve)

Bypass completo del tooltip por defecto. Se define una función que construye el texto manualmente usando los datos originales:

```csharp
// Guardar referencia a los datos ordenados para acceso desde el formatter
var orderedData = ordered; // ya existe

new RowSeries<double>
{
    Values = values,
    TooltipLabelFormatter = point =>
    {
        int idx = (int)point.SecondaryValue; // Y position = index
        var dto = orderedData[idx];
        return $"{dto.LicensePlate}\n${dto.CostPerKm:F3}/km";
    },
    ...
}
```

### Paso 3: Layout manual (fallback final)

Si LiveCharts sigue sin funcionar, reemplazar los 2 `CartesianChart` RowSeries por `CollectionView` + template con `BoxView` proporcional + `Label`. Control total sin dependencia de la librería.

## Archivos afectados

| Paso | Archivo | Cambio |
|---|---|---|
| 1 | `BiDashboardViewModel.cs:222-229` | Agregar `Mapping = (v, i) => new Coordinate(v, i)` a CostPerKm |
| 1 | `BiDashboardViewModel.cs:264-271` | Agregar `Mapping = (v, i) => new Coordinate(v, i)` a EmergencyRate |
| 2 | `BiDashboardViewModel.cs` | Agregar `TooltipLabelFormatter` si Mapping falla |
| 3 | `BiDashboardPage.xaml` + `.cs` | Reemplazar CartesianChart por CollectionView |
