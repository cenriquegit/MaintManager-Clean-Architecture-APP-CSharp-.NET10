using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaintManager.MAUI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private string? _authToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        ApplySavedBaseUrl();
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token").ConfigureAwait(false);
        if (string.IsNullOrEmpty(token))
            token = Preferences.Get("auth_token", null);

        if (string.IsNullOrEmpty(token))
            return false;

        var expiresAtStr = Preferences.Get("session_expires_at", null);
        if (expiresAtStr is not null
            && DateTime.TryParseExact(expiresAtStr, "O", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt)
            && DateTime.UtcNow >= expiresAt)
        {
            ClearAuthToken();
            SecureStorage.Remove("auth_token");
            Preferences.Remove("auth_token");
            Preferences.Remove("user_username");
            Preferences.Remove("user_fullname");
            Preferences.Remove("user_role");
            Preferences.Remove("session_expires_at");
            return false;
        }

        SetAuthToken(token);
        return true;
    }

    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5056"
            : "http://localhost:5056";

    public void ApplySavedBaseUrl()
    {
        var savedUrl = Preferences.Get("api_url", DefaultBaseUrl);
        _httpClient.BaseAddress = new Uri(savedUrl.TrimEnd('/') + "/");
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

    public async Task<byte[]?> GetByteArrayAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsByteArrayAsync();

        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error {response.StatusCode}: {error}");
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data is not null)
            content = JsonContent.Create(data, options: _jsonOptions);
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAndUnwrapAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data is not null)
            content = JsonContent.Create(data, options: _jsonOptions);
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
        HttpContent? content = null;
        if (data is not null)
            content = JsonContent.Create(data, options: _jsonOptions);
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