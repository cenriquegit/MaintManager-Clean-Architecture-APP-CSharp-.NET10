using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class VehiclesTests : TestBase
{
    public VehiclesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/vehicles");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminAuth_ReturnsVehicleList()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<VehicleListResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
        Assert.NotEmpty(wrapper.Data);
    }

    [Fact]
    public async Task GetAll_WithTecnicoAuth_ReturnsVehicleList()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<VehicleListResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsVehicle()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var allWrapper = await Client.GetFromJsonAsync<VehicleListResponse>(
            "/api/v1/vehicles", JsonOptions);
        Assert.NotNull(allWrapper?.Data);
        Assert.NotEmpty(allWrapper.Data);
        var firstId = allWrapper.Data[0].Prcoid;

        var response = await Client.GetAsync($"/api/v1/vehicles/{firstId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<VehicleDetailResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/vehicles/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentKm_WithValidId_ReturnsKm()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var allWrapper = await Client.GetFromJsonAsync<VehicleListResponse>(
            "/api/v1/vehicles", JsonOptions);
        Assert.NotNull(allWrapper?.Data);
        var firstId = allWrapper.Data[0].Prcoid;

        var response = await Client.GetAsync($"/api/v1/vehicles/{firstId}/current-km");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<IntResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    [Fact]
    public async Task GetSchedule_WithValidId_ReturnsSchedule()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var allWrapper = await Client.GetFromJsonAsync<VehicleListResponse>(
            "/api/v1/vehicles", JsonOptions);
        Assert.NotNull(allWrapper?.Data);
        var firstId = allWrapper.Data[0].Prcoid;

        var response = await Client.GetAsync($"/api/v1/vehicles/{firstId}/schedule");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<ScheduleResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    private sealed class VehicleListResponse
    {
        public bool Success { get; set; }
        public List<VehicleItem>? Data { get; set; }
    }

    private sealed class VehicleItem
    {
        public int Prcoid { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
    }

    private sealed class VehicleDetailResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
    }

    private sealed class IntResponse
    {
        public bool Success { get; set; }
        public int Data { get; set; }
    }

    private sealed class ScheduleResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
    }
}
