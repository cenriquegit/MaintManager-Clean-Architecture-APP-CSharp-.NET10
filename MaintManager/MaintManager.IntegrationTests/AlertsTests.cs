using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class AlertsTests : TestBase
{
    public AlertsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetUnresolved_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/alerts");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUnresolved_WithAuth_ReturnsAlertList()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/alerts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<AlertsResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task CheckAlerts_WithoutAdminRole_Returns403()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.PostAsync("/api/v1/alerts/check", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CheckAlerts_WithAdminAuth_Returns200()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.PostAsync("/api/v1/alerts/check", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<SimpleResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    [Fact]
    public async Task MarkRead_WithValidId_Returns200()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var listWrapper = await Client.GetFromJsonAsync<AlertsResponse>(
            "/api/v1/alerts", JsonOptions);
        var alertId = listWrapper?.Data?.FirstOrDefault()?.Alogid;

        if (alertId is null)
            return;

        var response = await Client.PutAsJsonAsync($"/api/v1/alerts/{alertId}/read", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class AlertsResponse
    {
        public bool Success { get; set; }
        public List<AlertItem>? Data { get; set; }
    }

    private sealed class AlertItem
    {
        public int Alogid { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class SimpleResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
    }
}
