using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class MaintenancesTests : TestBase
{
    public MaintenancesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/maintenances?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdminAuth_ReturnsPagedMaintenances()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/maintenances?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<PagedMaintenancesResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
        Assert.NotNull(wrapper.Data.Items);
        Assert.True(wrapper.Data.Page >= 1);
        Assert.True(wrapper.Data.PageSize > 0);
    }

    [Fact]
    public async Task GetAll_WithTecnicoAuth_ReturnsMaintenances()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.GetAsync("/api/v1/maintenances?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsMaintenance()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var listWrapper = await Client.GetFromJsonAsync<PagedMaintenancesResponse>(
            "/api/v1/maintenances?page=1&pageSize=1", JsonOptions);
        Assert.NotNull(listWrapper?.Data?.Items);

        if (listWrapper.Data.Items.Count == 0)
            return;

        var firstId = listWrapper.Data.Items[0].Mainid;

        var response = await Client.GetAsync($"/api/v1/maintenances/{firstId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detailWrapper = await response.Content.ReadFromJsonAsync<MaintenanceDetailResponse>(JsonOptions);
        Assert.NotNull(detailWrapper);
        Assert.True(detailWrapper.Success);
        Assert.NotNull(detailWrapper.Data);
    }

    [Fact]
    public async Task GetById_WithInvalidId_Returns404()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/maintenances/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByVehicle_WithValidVehicleId_ReturnsMaintenances()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var vehiclesWrapper = await Client.GetFromJsonAsync<VehiclesResponse>(
            "/api/v1/vehicles", JsonOptions);
        Assert.NotNull(vehiclesWrapper?.Data);

        if (vehiclesWrapper.Data.Count == 0)
            return;

        var vehicleId = vehiclesWrapper.Data[0].Prcoid;

        var response = await Client.GetAsync($"/api/v1/vehicles/{vehicleId}/maintenances");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<MaintenancesForVehicleResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    private sealed class PagedMaintenancesResponse
    {
        public bool Success { get; set; }
        public PagedData? Data { get; set; }
    }

    private sealed class PagedData
    {
        public List<MaintenanceItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class MaintenanceItem
    {
        public int Mainid { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private sealed class MaintenanceDetailResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
    }

    private sealed class VehiclesResponse
    {
        public bool Success { get; set; }
        public List<VehicleItem>? Data { get; set; }
    }

    private sealed class VehicleItem
    {
        public int Prcoid { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
    }

    private sealed class MaintenancesForVehicleResponse
    {
        public bool Success { get; set; }
        public List<object>? Data { get; set; }
    }
}
