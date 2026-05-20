using System.Text;
using System.Text.Json;
using MaintManager.Shared.Models;

namespace MaintManager.MAUI.Services;

public class AuthService
{
    private readonly ApiService _apiService;

    public AuthService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new { username, password };
        var response = await _apiService.PostAndUnwrapAsync<LoginResponse>("api/v1/auth/login", request);
        if (response is not null && !string.IsNullOrEmpty(response.Token))
        {
            _apiService.SetAuthToken(response.Token);
            await SecureStorage.SetAsync("auth_token", response.Token);
            Preferences.Set("auth_token", response.Token);
            Preferences.Set("user_username", response.Username);
            Preferences.Set("user_fullname", response.FullName);
            Preferences.Set("user_role", response.Role);
            Preferences.Set("user_workid", ExtractWorkidFromToken(response.Token));
            Preferences.Set("session_expires_at", DateTime.UtcNow.AddHours(8).ToString("O"));
            return true;
        }
        return false;
    }

    public string? GetUsername() => Preferences.Get("user_username", null);
    public string? GetFullName() => Preferences.Get("user_fullname", null);
    public string? GetRole() => Preferences.Get("user_role", null);
    public int GetWorkid() => Preferences.Get("user_workid", 0);
    public bool IsAdmin() => string.Equals(GetRole(), "Administrador", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(GetRole(), "Admin", StringComparison.OrdinalIgnoreCase);

    private static int ExtractWorkidFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return 0;
            var payload = parts[1];
            var mod = payload.Length % 4;
            var padded = mod switch
            {
                2 => payload + "==",
                3 => payload + "=",
                _ => payload
            };
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(
                padded.Replace('-', '+').Replace('_', '/')));
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("nameid", out var el) ? el.GetInt32() : 0;
        }
        catch
        {
            return 0;
        }
    }

    public void Logout()
    {
        _apiService.ClearAuthToken();
        SecureStorage.Remove("auth_token");
        Preferences.Remove("auth_token");
        Preferences.Remove("user_username");
        Preferences.Remove("user_fullname");
        Preferences.Remove("user_role");
        Preferences.Remove("user_workid");
        Preferences.Remove("session_expires_at");
    }
}
