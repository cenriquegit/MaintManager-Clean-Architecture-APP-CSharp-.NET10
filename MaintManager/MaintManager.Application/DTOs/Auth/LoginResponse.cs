namespace MaintManager.Application.DTOs.Auth;

public sealed record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);