# Plan: Fix Expiring Lots Pie Chart — tooltip y valores

## Diagnóstico

### Código actual (`BiDashboardViewModel.cs:358-384`)

```csharp
Name = $"Crítico (=7d) — {critical:F0}",   // valor incrustado + redondeo F0
Name = $"Próximo (=30d) — {warning:F0}",
Name = $"Normal (>30d) — {normal:F0}",
```

Dos problemas:
1. **Valor duplicado en tooltip**: `Name` ya tiene el número (`87`), y LiveCharts vuelve a mostrar el valor del slice (`87.486`)
2. **Decimales feos**: El slice tiene `87.486` (suma de `decimal` convertido a `double`)

## Solución

1. **Quitar el valor del `Name`** — solo etiqueta descriptiva
2. **Redondear los valores a enteros** — `Math.Round()` sobre la suma antes de asignarla. Son cantidades de material, no tiene sentido `87.486` unidades.

```csharp
var critical = Math.Round(data.Where(...).Sum(l => (double)l.CurrentQuantity));
var warning  = Math.Round(data.Where(...).Sum(l => (double)l.CurrentQuantity));
var normal   = Math.Round(data.Where(...).Sum(l => (double)l.CurrentQuantity));

// Names limpios, sin valor incrustado
Name = "Crítico (≤7d)",
Name = "Próximo (≤30d)",
Name = "Normal (>30d)",
```

LiveCharts mostrará el valor redondeado automáticamente en tooltip y leyenda, sin duplicación.

## Archivo

| Archivo | Líneas | Cambio |
|---|---|---|
| `BiDashboardViewModel.cs` | 354-356 | `Math.Round()` en las 3 sumas |
| `BiDashboardViewModel.cs` | 363, 371, 379 | Quitar `— {valor:F0}` del `Name` |
