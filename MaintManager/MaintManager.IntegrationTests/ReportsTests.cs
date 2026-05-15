using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class ReportsTests : TestBase
{
    public ReportsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDashboard_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/reports/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithAuth_ReturnsSummary()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task GetDashboard_WithTecnicoAuth_ReturnsSummary()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCostPerKm_WithoutAdminRole_Returns403()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/cost-per-km");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCostPerKm_WithAdminAuth_ReturnsData()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/cost-per-km");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<CostPerKmResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    [Fact]
    public async Task GetEmergencyRate_WithAdminAuth_ReturnsData()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/emergency-rate");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMonthlyCost_WithAdminAuth_ReturnsData()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/monthly-cost?months=6");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCalendarCompliance_WithAdminAuth_ReturnsData()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/calendar-compliance");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExportMaintenancePdf_WithValidId_ReturnsPdf()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var listWrapper = await Client.GetFromJsonAsync<MaintenancesResponse>(
            "/api/v1/maintenances?page=1&pageSize=1", JsonOptions);
        var maintId = listWrapper?.Data?.Items?.FirstOrDefault()?.Mainid;

        if (maintId is null)
            return;

        var response = await Client.GetAsync($"/api/v1/reports/maintenances/{maintId}/pdf");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExportCostExcel_WithAdminAuth_ReturnsExcel()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/cost-excel");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExportCostExcel_WithoutAdminRole_Returns403()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/reports/cost-excel");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed class DashboardResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
    }

    private sealed class CostPerKmResponse
    {
        public bool Success { get; set; }
        public List<object>? Data { get; set; }
    }

    private sealed class MaintenancesResponse
    {
        public PagedMaintenancesData? Data { get; set; }
    }

    private sealed class PagedMaintenancesData
    {
        public List<MaintenanceItem>? Items { get; set; }
    }

    private sealed class MaintenanceItem
    {
        public int Mainid { get; set; }
    }
}
