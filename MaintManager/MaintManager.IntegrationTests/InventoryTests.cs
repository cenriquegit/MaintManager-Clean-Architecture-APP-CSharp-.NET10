using System.Net;
using System.Net.Http.Json;

namespace MaintManager.IntegrationTests;

public sealed class InventoryTests : TestBase
{
    public InventoryTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMaterials_WithoutAuth_Returns401()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/v1/inventory/materials");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMaterials_WithAuth_ReturnsMaterialList()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/inventory/materials");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<MaterialsResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
        Assert.NotNull(wrapper.Data);
    }

    [Fact]
    public async Task GetMaterialById_WithValidId_ReturnsMaterial()
    {
        SetAuthToken(await GetAdminTokenAsync());

        var listWrapper = await Client.GetFromJsonAsync<MaterialsResponse>(
            "/api/v1/inventory/materials", JsonOptions);
        Assert.NotNull(listWrapper?.Data);

        if (listWrapper.Data.Count == 0)
            return;

        var firstId = listWrapper.Data[0].Mateid;

        var response = await Client.GetAsync($"/api/v1/inventory/materials/{firstId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMaterialById_WithInvalidId_Returns404()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/inventory/materials/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLowStock_ReturnsMaterials()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/inventory/low-stock");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<MaterialsResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    [Fact]
    public async Task GetExpiringLots_ReturnsLots()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.GetAsync("/api/v1/inventory/expiring-lots?days=30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var wrapper = await response.Content.ReadFromJsonAsync<LotsResponse>(JsonOptions);
        Assert.NotNull(wrapper);
        Assert.True(wrapper.Success);
    }

    [Fact]
    public async Task CreateMaterial_WithoutAdminRole_Returns403()
    {
        SetAuthToken(await GetTecnicoTokenAsync());
        var response = await Client.PostAsJsonAsync("/api/v1/inventory/materials",
            new { macaid = 1, name = "Test", unitOfMeasure = "UN", stockMinimum = 5 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateMaterial_WithAdminAuth_Returns201()
    {
        SetAuthToken(await GetAdminTokenAsync());
        var response = await Client.PostAsJsonAsync("/api/v1/inventory/materials",
            new { macaid = 1, name = "Material Test", unitOfMeasure = "UN", stockMinimum = 5 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private sealed class MaterialsResponse
    {
        public bool Success { get; set; }
        public List<MaterialItem>? Data { get; set; }
    }

    private sealed class MaterialItem
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class LotsResponse
    {
        public bool Success { get; set; }
        public List<object>? Data { get; set; }
    }
}
