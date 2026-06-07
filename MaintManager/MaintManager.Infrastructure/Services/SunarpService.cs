using MaintManager.Domain.Interfaces.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace MaintManager.Infrastructure.Services;

public sealed class SunarpService : ISunarpService
{
    private readonly HttpClient _httpClient;

    private const string BaseUrl = "https://consultavehicular.sunarp.gob.pe";
    private const string ConsultaUrl = "https://consultavehicular.sunarp.gob.pe/consulta-vehicular";

    public SunarpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SunarpCaptchaResult> GetCaptchaAsync(string plate, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "es-PE,es;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");

            var response = await _httpClient.GetAsync(ConsultaUrl, ct);
            var html = await response.Content.ReadAsStringAsync(ct);

            // Try multiple captcha patterns
            var patterns = new[]
            {
                @"<img[^>]*?id=""captcha""[^>]*?src=""([^""]*)""",
                @"<img[^>]*?src=""([^""]*captcha[^""]*)""",
                @"<img[^>]*?src=""([^""]*Captcha[^""]*)""",
                @"<img[^>]*?class=""[^""]*captcha[^""]*""[^>]*?src=""([^""]*)""",
                @"src=""(/[^""]*captcha[^""]*\.(?:png|jpg|jpeg|gif))""",
            };

            string? captchaSrc = null;
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    captchaSrc = match.Groups[1].Value;
                    break;
                }
            }

            if (captchaSrc != null)
            {
                if (captchaSrc.StartsWith("/"))
                    captchaSrc = BaseUrl + captchaSrc;

                var imgResponse = await _httpClient.GetAsync(captchaSrc, ct);
                var imgBytes = await imgResponse.Content.ReadAsByteArrayAsync(ct);
                var captchaBase64 = Convert.ToBase64String(imgBytes);

                return new SunarpCaptchaResult(true, captchaBase64, null);
            }

            return new SunarpCaptchaResult(false, null,
                "No se pudo detectar el captcha en la página de SUNARP. El sitio puede haber cambiado su estructura o requerir navegador real. Ingresa los datos del vehículo manualmente.");
        }
        catch (HttpRequestException)
        {
            return new SunarpCaptchaResult(false, null,
                "No se puede acceder a SUNARP. Verifica tu conexión a internet. Ingresa los datos manualmente.");
        }
        catch (Exception ex)
        {
            return new SunarpCaptchaResult(false, null,
                $"Error de conexión con SUNARP: {ex.Message}. Ingresa los datos manualmente.");
        }
    }

    public async Task<SunarpVehicleData> ConsultAsync(string plate, string captchaCode, CancellationToken ct = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "es-PE,es;q=0.9");

            var getResponse = await _httpClient.GetAsync(ConsultaUrl, ct);
            var getHtml = await getResponse.Content.ReadAsStringAsync(ct);

            var tokenMatch = Regex.Match(getHtml, @"<input[^>]*?name=""_token""[^>]*?value=""([^""]*)""", RegexOptions.IgnoreCase);
            var csrfToken = tokenMatch.Success ? tokenMatch.Groups[1].Value : null;

            var formData = new Dictionary<string, string>
            {
                ["placa"] = plate.ToUpper(),
                ["captcha"] = captchaCode
            };
            if (csrfToken != null) formData["_token"] = csrfToken;

            var content = new FormUrlEncodedContent(formData);
            var postResponse = await _httpClient.PostAsync(ConsultaUrl, content, ct);
            var resultHtml = await postResponse.Content.ReadAsStringAsync(ct);
            var resultText = WebUtility.HtmlDecode(Regex.Replace(resultHtml, @"<[^>]+>", " ")).ToLower();

            if (resultText.Contains("no se encontró") || resultText.Contains("no existe") || resultText.Contains("incorrecto"))
                return new SunarpVehicleData(false, null, null, null, null, null, null, null,
                    "Placa no encontrada o captcha incorrecto.");

            return new SunarpVehicleData(
                true,
                Extract(resultText, @"marca[:\s]*([^\n<]+)"),
                Extract(resultText, @"modelo[:\s]*([^\n<]+)"),
                int.TryParse(Extract(resultText, @"a[ñn]o[:\s]*(\d{4})"), out var y) ? y : null,
                Extract(resultText, @"color[:\s]*([^\n<]+)"),
                Extract(resultText, @"(?:vin|n[uú]mero\s+vin)[:\s]*([A-Z0-9]+)"),
                Extract(resultText, @"(?:motor|n[uú]mero\s+de\s+motor)[:\s]*([^\n<]+)"),
                Extract(resultText, @"(?:titular|propietario)[:\s]*([^\n<]+)"),
                null
            );
        }
        catch (Exception ex)
        {
            return new SunarpVehicleData(false, null, null, null, null, null, null, null, $"Error: {ex.Message}");
        }
    }

    private static string? Extract(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
