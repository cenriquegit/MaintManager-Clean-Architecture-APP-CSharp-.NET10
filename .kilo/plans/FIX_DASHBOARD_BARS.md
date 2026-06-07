# PLAN: Restaurar dashboard + aplicar x1000 + LabelsRotation ajustado

## Diagnóstico forense completo

### Problema: Barras de Costo/km no visibles
El commit `c95af1c` (funcionaba) usaba `LabelsRotation = -20` y valores originales.
El commit `f94150c` (roto) y mis cambios posteriores usan `LabelsRotation = -90`.

**Causa raíz:** `LabelsRotation = -90°` con 10 vehículos hace que el eje X ocupe ~70+ px de altura. El `HeightRequest="220"` se queda sin espacio para el área de trazado → las barras se renderizan fuera del área visible.

### Cambios a aplicar
1. `LabelsRotation = -90` → `-45` en CostPerKm, MonthlyCost, Compliance (balance entre legibilidad y espacio)
2. Mantener `×1000` y `$/1000km` y `F2` (ya están)
3. Verificar que `DataLabels` en RowSeries no interfiera con el eje Y de emergencia
