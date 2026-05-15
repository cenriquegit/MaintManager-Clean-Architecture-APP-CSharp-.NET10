using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaintManager.IntegrationTests;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private string? _adminToken;
    private string? _tecnicoToken;

    protected TestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        _adminToken ??= await GetTokenAsync("herror.ortiz", "Admin2026!");
        return _adminToken;
    }

    protected async Task<string> GetTecnicoTokenAsync()
    {
        _tecnicoToken ??= await GetTokenAsync("juan.quispe", "Tecnico2026!");
        return _tecnicoToken;
    }

    private async Task<string> GetTokenAsync(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username, password });

        response.EnsureSuccessStatusCode();

        var wrapper = await response.Content.ReadFromJsonAsync<LoginResponseWrapper>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
        Assert.False(string.IsNullOrEmpty(wrapper.Data.Token));

        return wrapper.Data.Token;
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    private sealed class LoginResponseWrapper
    {
        public bool Success { get; set; }
        public LoginData? Data { get; set; }
        public string? Message { get; set; }
    }

    private sealed class LoginData
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
