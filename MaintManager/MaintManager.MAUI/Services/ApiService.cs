using System.Diagnostics;
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
        var url = Preferences.Get("api_url", DefaultBaseUrl);
        Debug.WriteLine($"[MaintManager] ApiService ctor. Base URL: {url}");
        ApplySavedBaseUrl();
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token").ConfigureAwait(false);
        if (string.IsNullOrEmpty(token))
            token = Preferences.Get("auth_token", null);

        if (string.IsNullOrEmpty(token))
        {
            Debug.WriteLine("[MaintManager] TryRestoreSession: no token found");
            return false;
        }

        var expiresAtStr = Preferences.Get("session_expires_at", null);
        Debug.WriteLine($"[MaintManager] TryRestoreSession: token={token[..Math.Min(20, token.Length)]}..., expires={expiresAtStr}");

        if (expiresAtStr is not null
            && DateTime.TryParseExact(expiresAtStr, "O", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt)
            && DateTime.UtcNow >= expiresAt)
        {
            Debug.WriteLine("[MaintManager] TryRestoreSession: session EXPIRED");
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
        Debug.WriteLine("[MaintManager] TryRestoreSession: session restored OK");
        return true;
    }

    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5056"
            : "http://localhost:5056";

    public void ApplySavedBaseUrl()
    {
        var savedUrl = Preferences.Get("api_url", DefaultBaseUrl);
        Debug.WriteLine($"[MaintManager] ApplySavedBaseUrl: {savedUrl}");
        _httpClient.BaseAddress = new Uri(savedUrl.TrimEnd('/') + "/");
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Debug.WriteLine($"[MaintManager] SetAuthToken: Bearer {token[..Math.Min(20, token.Length)]}...");
    }

    public void ClearAuthToken()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        Debug.WriteLine("[MaintManager] ClearAuthToken");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        Debug.WriteLine($"[MaintManager] GET {endpoint}");
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response, endpoint);
    }

    public async Task<byte[]?> GetByteArrayAsync(string endpoint)
    {
        Debug.WriteLine($"[MaintManager] GET (bytes) {endpoint}");
        var response = await _httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Debug.WriteLine($"[MaintManager] GET OK ({bytes.Length} bytes)");
            return bytes;
        }

        var error = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[MaintManager] GET ERROR {response.StatusCode}: {Truncate(error)}");
        throw new HttpRequestException($"Error {response.StatusCode}: {error}");
    }

    public async Task<byte[]?> PostByteArrayAsync(string endpoint, object? data = null)
    {
        Debug.WriteLine($"[MaintManager] POST (bytes) {endpoint}");
        var content = data is not null
            ? JsonContent.Create(data, options: _jsonOptions)
            : null;
        var response = await _httpClient.PostAsync(endpoint, content);
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Debug.WriteLine($"[MaintManager] POST OK ({bytes.Length} bytes)");
            return bytes;
        }

        var error = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[MaintManager] POST ERROR {response.StatusCode}: {Truncate(error)}");
        throw new HttpRequestException($"Error {response.StatusCode}: {error}");
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data is not null)
        {
            content = JsonContent.Create(data, options: _jsonOptions);
            var body = await content.ReadAsStringAsync();
            Debug.WriteLine($"[MaintManager] POST {endpoint} body={Truncate(body)}");
        }
        else
        {
            Debug.WriteLine($"[MaintManager] POST {endpoint} (sin body)");
        }
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleResponse<T>(response, endpoint);
    }

    public async Task<T?> PostAndUnwrapAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data is not null)
        {
            content = JsonContent.Create(data, options: _jsonOptions);
            var body = await content.ReadAsStringAsync();
            Debug.WriteLine($"[MaintManager] POST (unwrap) {endpoint} body={Truncate(body)}");
        }
        else
        {
            Debug.WriteLine($"[MaintManager] POST (unwrap) {endpoint} (sin body)");
        }
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleWrappedResponse<T>(response, endpoint);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data is not null)
        {
            content = JsonContent.Create(data, options: _jsonOptions);
            var body = await content.ReadAsStringAsync();
            Debug.WriteLine($"[MaintManager] PUT {endpoint} body={Truncate(body)}");
        }
        else
        {
            Debug.WriteLine($"[MaintManager] PUT {endpoint} (sin body)");
        }
        var response = await _httpClient.PutAsync(endpoint, content);
        return await HandleResponse<T>(response, endpoint);
    }

    // ── Manejo de respuestas ──────────────────────────────────────

    private static async Task<T?> HandleResponse<T>(HttpResponseMessage response, string? endpoint = null)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"[MaintManager] {endpoint} → {response.StatusCode} body={Truncate(body)}");
            return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            Debug.WriteLine($"[MaintManager] ⛔ {endpoint} → ERROR {response.StatusCode} body={Truncate(body)}");
            throw new HttpRequestException($"Error {response.StatusCode}: {body}");
        }
    }

    private static async Task<T?> HandleWrappedResponse<T>(HttpResponseMessage response, string? endpoint = null)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponseWrapper<T>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (wrapper?.Success == true)
            {
                Debug.WriteLine($"[MaintManager] {endpoint} → {response.StatusCode} OK data={Truncate(body)}");
                return wrapper.Data;
            }
            Debug.WriteLine($"[MaintManager] {endpoint} → {response.StatusCode} API_ERROR msg={wrapper?.Message}");
            throw new HttpRequestException(wrapper?.Message ?? "Error desconocido");
        }
        else
        {
            Debug.WriteLine($"[MaintManager] ⛔ {endpoint} → ERROR {response.StatusCode} body={Truncate(body)}");
            throw new HttpRequestException($"Error {response.StatusCode}: {body}");
        }
    }

    // ── Utils ─────────────────────────────────────────────────────

    private static string Truncate(string s, int max = 500) =>
        s.Length <= max ? s : s[..max] + "...";

    private sealed class ApiResponseWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}