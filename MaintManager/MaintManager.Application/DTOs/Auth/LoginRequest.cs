namespace MaintManager.Application.DTOs.Auth;

public sealed record LoginRequest(string Username, string Password);

/// <summary>Respuesta con token JWT y datos del usuario autenticado.</summary>
