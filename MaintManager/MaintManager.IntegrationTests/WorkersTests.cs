using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class WorkersTests : TestBase
{
    public WorkersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetTechnicians_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/workers/technicians");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTechnicians_WithAdminAuth_ReturnsTechnicians()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/workers/technicians");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<TechniciansResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task GetTechnicians_WithTecnicoAuth_ReturnsTechnicians()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/workers/technicians");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTechnicians_ReturnsOnlyActiveMechanics()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetFromJsonAsync<TechniciansResponse>(
            "/api/v1/workers/technicians", JsonOptions);

        Assert.NotNull(response?.Data);

        foreach (var tech in response.Data)
        {
            Assert.True(tech.Workid > 0);
            Assert.False(string.IsNullOrWhiteSpace(tech.FullName));
        }
    }

    private sealed class TechniciansResponse
    {
        public bool Success { get; set; }
        public List<TechnicianItem>? Data { get; set; }
    }

    private sealed class TechnicianItem
    {
        public int Workid { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
