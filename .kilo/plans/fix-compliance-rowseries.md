# Plan: Convertir "Desviación de Calendario" de ColumnSeries a RowSeries

## Problema

Igual que Costo por Km y Tasa de Emergencia: `ColumnSeries` (barras verticales) con 10 vehículos en el eje X. Solo se ven ~5 por falta de espacio horizontal. Etiquetas rotadas desalineadas.

## Solución

Convertir a `RowSeries<ObservablePoint>` (barras horizontales), mismo patrón que los otros gráficos ya corregidos.

### Cambios en `BiDashboardViewModel.cs`

**1. Agregar propiedad de altura** (después de línea 95):
```csharp
[ObservableProperty]
private double _complianceHeight = 220;
```

**2. Reemplazar `BuildComplianceChart`** (líneas 387-437):

```csharp
private void BuildComplianceChart(List<ComplianceDto> data)
{
    var topDeviations = data.OrderByDescending(d => Math.Abs(d.KmDeviation)).Take(10).ToList();
    var labels = topDeviations.Select(d => d.LicensePlate).ToArray();

    var seriesList = new List<ISeries>();
    for (int i = 0; i < topDeviations.Count; i++)
    {
        var item = topDeviations[i];
        var color = item.ComplianceStatus switch
        {
            "Puntual" => Green,
            "Anticipado" => Orange,
            "Tardio" => Red,
            _ => Red
        };

        seriesList.Add(new RowSeries<ObservablePoint>
        {
            Values = [new ObservablePoint(item.KmDeviation, i)],
            Name = item.LicensePlate,
            Stroke = new SolidColorPaint(color),
            Fill = new SolidColorPaint(color),
            MaxBarWidth = 24,
        });
    }

    ComplianceSeries = seriesList.ToArray();

    ComplianceXAxes =
    [
        new Axis
        {
            Name = "Km de desviación (±)",
            TextSize = 12,
        }
    ];

    ComplianceYAxes =
    [
        new Axis
        {
            Labels = labels,
            TextSize = 12,
            MinStep = 1,
        }
    ];

    ComplianceHeight = 40 * topDeviations.Count;
}
```

Diferencias clave vs el código actual:
- `ColumnSeries<double>` con array de 10 posiciones → `RowSeries<ObservablePoint>` con 1 punto `(kmDeviation, index)`
- Eje X tenía labels (placas) → ahora eje Y tiene labels (placas)
- Eje Y tenía "Km de desviación (±)" → ahora eje X tiene eso
- Altura dinámica: `40 * count` = 400px para 10 vehículos
- Sin `LabelsRotation`

### Cambios en `BiDashboardPage.xaml` (línea 206)
```diff
- HeightRequest="220"
+ HeightRequest="{Binding ComplianceHeight}"
```

## Archivos

| Archivo | Cambio |
|---|---|
| `BiDashboardViewModel.cs:95` | Agregar `_complianceHeight` |
| `BiDashboardViewModel.cs:387-437` | `ColumnSeries` → `RowSeries<ObservablePoint>` + ejes invertidos |
| `BiDashboardPage.xaml:206` | `HeightRequest` binding |

## Nota

La leyenda debajo del chart (Puntual/Anticipado/Tardío) queda igual en el XAML, sin cambios.
