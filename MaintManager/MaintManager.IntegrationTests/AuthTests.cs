using System.Net.Http.Json;
using System.Net;

namespace MaintManager.IntegrationTests;

public sealed class AuthTests : TestBase
{
    public AuthTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "herror.ortiz", password = "Admin2026!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
        Assert.False(string.IsNullOrEmpty(wrapper.Data.Token));
        Assert.Equal("Admin", wrapper.Data.Role);
    }

    [Fact]
    public async Task Login_WithValidTecnicoCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "juan.quispe", password = "Tecnico2026!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
        Assert.Equal("Tecnico", wrapper.Data.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "herror.ortiz", password = "wrong_password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "nonexistent.user", password = "SomePass1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_TokenHasValidStructure()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "herror.ortiz", password = "Admin2026!" });

        var wrapper = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(wrapper?.Data?.Token);

        var parts = wrapper.Data.Token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task Login_ReturnsUsernameAndFullName()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new { username = "herror.ortiz", password = "Admin2026!" });

        var wrapper = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(wrapper?.Data);
        Assert.Equal("herror.ortiz", wrapper.Data.Username);
        Assert.False(string.IsNullOrEmpty(wrapper.Data.FullName));
    }

    private sealed class AuthResponse
    {
        public bool Success { get; set; }
        public AuthData? Data { get; set; }
    }

    private sealed class AuthData
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
