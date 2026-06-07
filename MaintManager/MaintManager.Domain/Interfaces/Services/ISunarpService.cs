namespace MaintManager.Domain.Interfaces.Services;

public interface ISunarpService
{
    Task<SunarpCaptchaResult> GetCaptchaAsync(string plate, CancellationToken ct = default);
    Task<SunarpVehicleData> ConsultAsync(string plate, string captchaCode, CancellationToken ct = default);
}

public sealed record SunarpCaptchaResult(
    bool Success,
    string? CaptchaBase64,
    string? Error
);

public sealed record SunarpVehicleData(
    bool Success,
    string? Brand,
    string? Model,
    int? Year,
    string? Color,
    string? Vin,
    string? EngineNumber,
    string? Titular,
    string? Error
);
