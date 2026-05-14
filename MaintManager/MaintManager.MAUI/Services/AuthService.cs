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
            return true;
        }
        return false;
    }

    public void Logout()
    {
        _apiService.ClearAuthToken();
        SecureStorage.Remove("auth_token");
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}