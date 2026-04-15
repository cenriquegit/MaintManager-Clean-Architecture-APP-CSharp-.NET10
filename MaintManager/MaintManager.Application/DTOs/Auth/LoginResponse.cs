public sealed record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);

// ── Common ────────────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Common;

/// <summary>
/// Wrapper de respuesta estandarizada para todos los endpoints.
/// Success=true + Data en casos exitosos.
/// Success=false + Message en casos de error.
/// </summary>
