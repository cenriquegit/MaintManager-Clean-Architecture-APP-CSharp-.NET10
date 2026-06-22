# Plan: Fix SIGSEGV crash — limpiar y reconstruir

## Causa raíz

Crash nativo `SIGSEGV` en `mono_class_get_field_from_name_full` durante `init_android_runtime`. Mono intenta acceder a una clase NULL — puntero nulo desreferenciado en offset 0x38.

Esto ocurre cuando hay ensamblados inconsistentes en los directorios `obj/` y `bin/`:
- DLLs compiladas con el viejo App ID (`com.companyname.maintmanager.maui`)
- DLLs compiladas con el nuevo `MaterialItemDto` (con campo `Type`)
- El archivo `.csproj` cambió App ID a `com.neocar.app`
- Algunos artefactos se recompilaron, otros no → inconsistencia

## Plan

### 1. Limpiar todos los artefactos de build

```powershell
# Shared
Remove-Item -Recurse -Force MaintManager/MaintManager.Shared/bin, MaintManager/MaintManager.Shared/obj -ErrorAction SilentlyContinue

# MAUI
Remove-Item -Recurse -Force MaintManager/MaintManager.MAUI/bin, MaintManager/MaintManager.MAUI/obj -ErrorAction SilentlyContinue
```

### 2. Reconstruir la app MAUI desde cero

```powershell
dotnet build MaintManager.MAUI/MaintManager.MAUI.csproj -f net10.0-android
```

### 3. Instalar APK y probar

El APK generado en `bin/Debug/net10.0-android/com.neocar.app-Signed.apk` o `com.neocar.app.apk`.

## Archivos a modificar

Ninguno. Solo limpieza y rebuild.
