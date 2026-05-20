namespace MaintManager.Shared.Models;

public sealed record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);
