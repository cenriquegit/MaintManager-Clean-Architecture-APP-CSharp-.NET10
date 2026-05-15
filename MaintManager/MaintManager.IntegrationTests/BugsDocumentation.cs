// ============================================================
// MaintManager — Bug & Fix Documentation
// ============================================================
// Este archivo documenta todos los bugs encontrados y
// corregidos durante el desarrollo del proyecto.
// ============================================================

namespace MaintManager.IntegrationTests;

/// <summary>
/// DOCUMENTACIÓN DE BUGS ENCONTRADOS Y CORREGIDOS
/// ==============================================
///
/// BUG #1 — Crash al navegar al Dashboard tras login exitoso
/// -----------------------------------------------------------
/// Síntoma:     Login retorna HTTP 200 con JWT, la app navega a //Dashboard
///              y se cierra inmediatamente sin mostrar error.
/// Causa raíz:  BaseViewModel._isEmpty = false por defecto (default de bool).
///              IsSuccess = !IsBusy && !HasError && !IsEmpty = true ANTES
///              de cargar datos. El ScrollView del Dashboard se renderiza
///              y los bindings KpiItems[0] a KpiItems[3] lanzan
///              ArgumentOutOfRangeException porque ObservableCollection
///              está vacía.
/// Fix:         BaseViewModel.cs:21 → _isEmpty = true
///
/// BUG #2 — ReportsPage nunca muestra contenido
/// ---------------------------------------------
/// Síntoma:     La página de Reportes siempre muestra el estado Empty,
///              nunca la lista de reportes disponibles.
/// Causa raíz:  ReportsViewModel.IsEmpty nunca se setea a false después
///              de poblar AvailableReports en el constructor.
///              IsSuccess = false permanentemente.
/// Fix:         ReportsViewModel.cs:37 → IsEmpty = AvailableReports.Count == 0;
///
/// BUG #3 — Error de conexión (Android HTTP bloqueado)
/// ---------------------------------------------------
/// Síntoma:     En Android (emulador y físico), la app muestra error de
///              conexión aunque la API esté corriendo.
/// Causa raíz:  Android 9+ bloquea tráfico HTTP cleartext por defecto.
///              El AndroidManifest.xml no tenía
///              android:usesCleartextTraffic="true".
/// Fix:         AndroidManifest.xml:3 → android:usesCleartextTraffic="true"
///
/// BUG #4 — URL hardcodeada 10.0.2.2 solo para emulador
/// -----------------------------------------------------
/// Síntoma:     En Windows, la app no conecta porque 10.0.2.2 no existe.
/// En físico, 10.0.2.2 tampoco funciona.
/// Causa raíz:  ApiService tenía URL hardcodeada http://10.0.2.2:5056
///              que solo funciona en Android Emulator.
/// Fix:         ApiService.ApplySavedBaseUrl() lee desde Preferences
///              con default por plataforma:
///              Android → http://10.0.2.2:5056
///              Windows → http://localhost:5056
///
/// BUG #5 — Cambios de URL en Settings no se aplicaban
/// ----------------------------------------------------
/// Síntoma:     El usuario cambiaba la URL en Settings pero seguía sin
///              conectar. Los cambios nunca se aplicaban al HttpClient.
/// Causa raíz:  SettingsViewModel guardaba en Preferences pero ApiService
///              nunca volvía a leer el valor.
/// Fix:         SettingsViewModel ahora inyecta ApiService y llama
///              ApplySavedBaseUrl() después de guardar.
///
/// BUG #6 — Mensajes de error en inglés sin formato
/// -------------------------------------------------
/// Síntoma:     Los errores de conexión mostraban mensajes crudos del
///              framework .NET en inglés (ej: "No connection could be
///              made...").
/// Causa raíz:  BaseViewModel.ExecuteAsync mostraba ex.Message
///              directamente.
/// Fix:         BaseViewModel ahora captura HttpRequestException y
///              TaskCanceledException con mensajes en español
///              diferenciando error de transporte vs error HTTP.
/// </summary>
public static class BugsDocumentation
{
    public static string GetAll() => @"
╔══════════════════════════════════════════════════════════╗
║              MAINTMANAGER — BUGS & FIXES               ║
╠══════════════════════════════════════════════════════════╣
║                                                        ║
║  BUG #1:  Crash al navegar al Dashboard                ║
║  ───────                                                ║
║  Síntoma: Login exitoso → app se cierra                ║
║  Causa:   _isEmpty = false → IsSuccess = true           ║
║           → KpiItems[0..3] en XAML lanza excepción     ║
║  Fix:     BaseViewModel._isEmpty = true                 ║
║  Archivo: ViewModels/BaseViewModel.cs:21                ║
║                                                        ║
║  BUG #2:  ReportsPage nunca muestra contenido           ║
║  ───────                                                ║
║  Síntoma: Página de reportes siempre vacía              ║
║  Causa:   IsEmpty nunca seteado a false                 ║
║  Fix:     IsEmpty = AvailableReports.Count == 0;        ║
║  Archivo: ViewModels/Reports/ReportsViewModel.cs:37     ║
║                                                        ║
║  BUG #3:  Android bloquea conexión HTTP                 ║
║  ───────                                                ║
║  Síntoma: Error de conexión en Android                  ║
║  Causa:   Android 9+ bloquea cleartext HTTP             ║
║  Fix:     usesCleartextTraffic='true' en Manifest       ║
║  Archivo: Platforms/Android/AndroidManifest.xml:3       ║
║                                                        ║
║  BUG #4:  URL hardcodeada solo para emulador            ║
║  ───────                                                ║
║  Síntoma: No conecta en Windows ni físico               ║
║  Causa:   10.0.2.2 solo funciona en emulador            ║
║  Fix:     URL desde Preferences con default por plataf. ║
║  Archivo: Services/ApiService.cs:18-21                  ║
║                                                        ║
║  BUG #5:  Settings URL no se aplicaba                   ║
║  ───────                                                ║
║  Síntoma: Cambiar URL en Settings no tiene efecto       ║
║  Causa:   Solo guardaba en Preferences                  ║
║  Fix:     ApiService.ApplySavedBaseUrl() al guardar     ║
║  Archivo: ViewModels/Settings/SettingsViewModel.cs:44   ║
║                                                        ║
║  BUG #6:  Mensajes de error en inglés                   ║
║  ───────                                                ║
║  Síntoma: Mensajes crudos del framework                 ║
║  Causa:   ex.Message mostrado directamente              ║
║  Fix:     Captura con mensajes en español               ║
║  Archivo: ViewModels/BaseViewModel.cs:39-55             ║
║                                                        ║
╚══════════════════════════════════════════════════════════╝
";
}
