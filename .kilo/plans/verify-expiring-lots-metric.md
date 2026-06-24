# Plan: Verificar qué cuenta el gráfico "Lotes Próximos a Vencer"

## Diagnóstico

### Flujo de datos

1. **API** (`BiReportService.cs:178-197`): consulta `MaterialLot` con `LotStatus = "activo"` y `ExpirationDate <= hoy + 60 días`
2. Cada fila es **UN LOTE** con:
   - `CurrentQuantity`: cantidad de material en ESE lote (ej: 10 unidades)
   - `DaysUntilExpiry`: días hasta que ESE lote vence
   - `AtRiskCost`: `CurrentQuantity × UnitCost`
3. **Cliente MAUI** (`BuildExpiringLotsChart`): agrupa por categoría de vencimiento y **SUMA** `CurrentQuantity`

```csharp
var critical = data.Where(l => l.DaysUntilExpiry <= 7).Sum(l => l.CurrentQuantity);
```

### Ejemplo con datos reales

| Lote | Material | Cantidad | Vence en |
|---|---|---|---|
| Lote A | Aceite 5W30 | 15 | 3 días |
| Lote B | Filtro aceite | 8 | 5 días |
| Lote C | Batería | 2 | 20 días |
| Lote D | Llanta | 4 | 45 días |

Con el código actual:
- Crítico (≤7d): 15 + 8 = **23** (suma de cantidades de Lote A + Lote B)
- Próximo (≤30d): 2 = **2** (Lote C)
- Normal (>30d): 4 = **4** (Lote D)

Si se contaran LOTES en vez de cantidades:
- Crítico: **2** lotes (A y B)
- Próximo: **1** lote (C)
- Normal: **1** lote (D)

### ¿Cuál es correcto?

| Métrica | Qué mide | Útil para |
|---|---|---|
| `.Count()` | Número de lotes por vencer | Saber cuántos lotes atender |
| `.Sum(l => l.CurrentQuantity)` | Cantidad total de material | Saber cuánto inventario está en riesgo |

El subtítulo dice **"Cantidad de material por estado de vencimiento"**, lo cual justifica la SUMA actual. Pero el título dice **"Lotes Próximos a Vencer"**, que suena a conteo de lotes.

### Próximo paso

Ejecutar consulta SQL para ver los lotes reales y sus cantidades, y preguntar al usuario cuál métrica prefiere: conteo de lotes o suma de cantidades de material.
