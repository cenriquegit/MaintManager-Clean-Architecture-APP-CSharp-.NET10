# Simplificar wizard y agregar asignación de técnico

## Cambios en ViewModel (`MaintenanceWizardViewModel.cs`)

1. `MaxSteps = 7` → `MaxSteps = 4`
2. Eliminar `Materials`, `Operations`, `DiagnosisCode`, `DiagnosisDescription`
3. Agregar `Technicians`, `SelectedTechnician`, `MechanicNote`
4. Eliminar `LoadMaterialsAsync()`, `LoadDefaultOperations()`
5. Agregar `LoadTechniciansAsync()`
6. En `LoadVehicles()`: quitar llamadas a `LoadMaterialsAsync()` y `LoadDefaultOperations()`, agregar `await LoadTechniciansAsync()`
7. En `Save()`: usar `SelectedTechnician?.Workid ?? _authService.GetWorkid()` en vez de `_authService.GetWorkid()`, usar `MechanicNote` en vez de `DiagnosisDescription`

## Cambios en XAML (`MaintenanceWizardPage.xaml`)

1. Step indicator: `{0}/7` → `{0}/4`
2. Eliminar Step 3 (Componentes y Materiales), Step 4 (Operaciones), Step 5 (Diagnóstico), Step 6 (Próximo Servicio)
3. Agregar nuevo Step 3 (Asignar Técnico):
   - Label "Asignar Técnico"
   - Picker con `{Binding Technicians}` vinculado a `{Binding SelectedTechnician}`
   - Label "Nota para el mecánico (opcional)"
   - Editor con `{Binding MechanicNote}`
4. En Step 4 (Confirmación): quitar línea `Diagnóstico: {Binding DiagnosisCode}`

## Orden de ejecución

```bash
dotnet build MaintManager.MAUI/MaintManager.MAUI.csproj -f net10.0-android -c Release
```
