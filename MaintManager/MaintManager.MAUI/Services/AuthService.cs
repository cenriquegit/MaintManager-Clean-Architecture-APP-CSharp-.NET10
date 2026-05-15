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
            Preferences.Set("user_username", response.Username);
            Preferences.Set("user_fullname", response.FullName);
            Preferences.Set("user_role", response.Role);
            return true;
        }
        return false;
    }

    public string? GetUsername() => Preferences.Get("user_username", null);
    public string? GetFullName() => Preferences.Get("user_fullname", null);
    public string? GetRole() => Preferences.Get("user_role", null);
    public bool IsAdmin() => string.Equals(GetRole(), "Administrador", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(GetRole(), "Admin", StringComparison.OrdinalIgnoreCase);

    public void Logout()
    {
        _apiService.ClearAuthToken();
        SecureStorage.Remove("auth_token");
        Preferences.Remove("user_username");
        Preferences.Remove("user_fullname");
        Preferences.Remove("user_role");
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
