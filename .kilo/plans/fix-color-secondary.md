# Fix crash: ColorSecondary not found

## Problema
`LoginPage.xaml` usa `{StaticResource ColorSecondary}` pero el recurso no está definido en `App.xaml`.

## Fix
Agregar en `C:\Users\carlo\Desktop\proyect\MaintManager\MaintManager.MAUI\App.xaml` línea 17:

```xml
<Color x:Key="ColorSecondary">#0D47A1</Color>
```

Después de `ColorPrimary` (línea 16), antes de `ColorPrimaryDark` (Actual línea 17).

## Build
```bash
dotnet build MaintManager.MAUI/MaintManager.MAUI.csproj -f net10.0-android -c Release
```
