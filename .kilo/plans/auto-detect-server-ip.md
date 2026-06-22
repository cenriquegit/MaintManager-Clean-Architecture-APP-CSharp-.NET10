# Plan: Auto-detección con verificación manual (v2 — fix crash)

## Causa raíz del crash

`NetworkInterface.GetAllNetworkInterfaces()` en Android causa un crash nativo (JNI/SIGSEGV) que el `try/catch` de C# no puede atrapar. Este método está en `GetCandidateUrls()` en `ApiService.cs`.

## Objetivo

La app detecta automáticamente el IPv4 del servidor al iniciar y lo muestra en el campo URL del LoginPage. El usuario verifica que sea correcto y presiona "Guardar URL" para aplicarlo.

## Cambios

### 1. `ApiService.cs` — Reemplazar `GetCandidateUrls()` con UDP socket trick

**Eliminar:**
- `using System.Net.NetworkInformation;` (ya no se necesita)
- `NetworkInterface.GetAllNetworkInterfaces()` y todo el código asociado con `NetworkInterface`, `IPInterfaceProperties`, `GatewayAddresses`
- `#pragma warning disable CA1416` / `#pragma warning restore CA1416`

**Reemplazar `GetCandidateUrls()` con:**
```csharp
private static List<string> GetCandidateUrls()
{
    var urls = new List<string>();
    try
    {
        // UDP socket trick: "conecta" a una IP externa sin enviar datos
        // para obtener el IPv4 local de la interfaz activa
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var localEndPoint = (IPEndPoint)socket.LocalEndPoint!;
        var ip = localEndPoint.Address.ToString();

        var parts = ip.Split('.');
        if (parts.Length != 4) return urls;

        var subnet = $"{parts[0]}.{parts[1]}.{parts[2]}";

        var candidates = new[] { $"{subnet}.1", $"{subnet}.33", $"{subnet}.100" };
        foreach (var c in candidates)
        {
            urls.Add($"http://{c}:5056");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[MaintManager] GetCandidateUrls error: {ex.Message}");
    }
    return urls;
}
```

### 2. `App.xaml.cs` — Restaurar `window.Created` con Task.Run

El auto-detect se ejecuta en background thread para no bloquear el UI y aislar cualquier posible crash:

```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    var window = new Window(new AppShell());

    window.Created += (_, _) =>
    {
        Task.Run(async () =>
        {
            try
            {
                var apiService = MauiProgram.Services?.GetService<ApiService>();
                if (apiService is not null)
                    await apiService.TryAutoDetectBaseUrl();
            }
            catch { }
        });
    };

    return window;
}
```

### 3. `ApiService.cs` — Sin cambios adicionales

- `ServerDetected` event: ya existe ✅
- `TryAutoDetectBaseUrl()`: ya no llama a `ApplySavedBaseUrl()` ✅
- `Preferences.Set("api_url", url)` + `ServerDetected?.Invoke(url)`: ya implementado ✅

## Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `ApiService.cs` | Reemplazar `GetCandidateUrls()` con UDP socket trick, quitar `using System.Net.NetworkInformation` |
| `App.xaml.cs` | Restaurar `window.Created` handler con `Task.Run` |

## No modificar

- `LoginPage.xaml` — ya restaurado ✅
- `LoginViewModel.cs` — ya restaurado + ServerDetected ✅
- `ConfigController.cs` — `[AllowAnonymous]` ya aplicado ✅

## Flujo resultante

```
App inicia
  │
  ├─ LoginPage aparece → muestra URL guardada o DefaultBaseUrl
  ├─ window.Created → Task.Run → auto-detect en background
  │     │
  │     ├─ UDP socket trick → obtiene IPv4 local (sin NetworkInterface)
  │     ├─ Arma candidatos (subnet.1, .33, .100)
  │     ├─ Prueba cada uno con GET /api/v1/config (3s timeout)
  │     │
  │     └─ Encuentra servidor → Preferences.Set + ServerDetected event
  │                                │
  │                                └─ LoginViewModel.ApiUrl = IP detectado
  │                                     │
  │                                     └─ LoginPage muestra IP en el Entry
  │
  └─ Usuario ve el IP detectado → verifica → presiona "Guardar URL" → aplica
```
