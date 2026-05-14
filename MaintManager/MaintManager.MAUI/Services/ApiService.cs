using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MaintManager.MAUI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://10.0.2.2:5056"); // ⚠️ Cambia si usas otra IP o puerto
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        var content = data is null ? null : new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAndUnwrapAsync<T>(string endpoint, object? data = null)
    {
        var content = data is null ? null : new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleWrappedResponse<T>(response);
    }

    private static async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {response.StatusCode}: {error}");
        }
    }

    private static async Task<T?> HandleWrappedResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponseWrapper<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (wrapper?.Success == true)
                return wrapper.Data;
            throw new HttpRequestException(wrapper?.Message ?? "Error desconocido");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {response.StatusCode}: {error}");
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? data = null)
    {
        var content = data is null ? null : new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

    private sealed class ApiResponseWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}