using FluentValidation;
using MaintManager.Application.DTOs.Auth;
using MaintManager.Application.DTOs.Common;
using MaintManager.Infrastructure.Data;
using MaintManager.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MaintManager.API.Controllers;

/// <summary>Autenticación de usuarios del sistema.</summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly FleetMaintenanceContext _context;
    private readonly IConfiguration _configuration;
    private readonly IValidator<LoginRequest> _validator;

    public AuthController(
        FleetMaintenanceContext context,
        IConfiguration configuration,
        IValidator<LoginRequest> validator)
    {
        _context = context;
        _configuration = configuration;
        _validator = validator;
    }

    /// <summary>Iniciar sesión con usuario y contraseña.</summary>
    /// <response code="200">Token JWT generado correctamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    /// <response code="401">Credenciales incorrectas o usuario bloqueado.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                ErrorMessages.General.ValidationError,
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var passwordHash = ComputeMd5(request.Password);

        var worker = await _context.Workers
            .Include(w => w.Person)
            .FirstOrDefaultAsync(w =>
                w.Username == request.Username &&
                w.Password == passwordHash &&
                w.Status, ct);

        if (worker is null)
            return Unauthorized(ApiResponse<object>.Fail(ErrorMessages.Auth.InvalidCredentials));

        if (worker.Locked)
            return Unauthorized(ApiResponse<object>.Fail(ErrorMessages.Auth.UserLocked));

        // Determinar rol según jobid
        // jobid de "Mecánico Técnico" → Tecnico; resto → Admin
        var role = await DetermineRoleAsync(worker.Jobid, ct);
        var (token, expiresAt) = GenerateJwt(worker.Workid, worker.Username!, role,
            worker.Person?.Name ?? string.Empty);

        var response = new LoginResponse(
            Token: token,
            Username: worker.Username!,
            FullName: $"{worker.Person?.Fln} {worker.Person?.Mln} {worker.Person?.Name}".Trim(),
            Role: role,
            ExpiresAt: expiresAt
        );

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    private async Task<string> DetermineRoleAsync(short jobid, CancellationToken ct)
    {
        var jobName = await _context.Jobs
            .Where(j => j.Jobid == jobid)
            .Select(j => j.Name)
            .FirstOrDefaultAsync(ct);

        return jobName?.Contains("Mecánico", StringComparison.OrdinalIgnoreCase) == true
            ? RoleNames.Tecnico
            : RoleNames.Admin;
    }

    private (string token, DateTime expiresAt) GenerateJwt(
        int workid, string username, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "8");
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, workid.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.GivenName, fullName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    // TODO: Migrar a algoritmo seguro (bcrypt/PBKDF2) - Actualmente usa MD5 por compatibilidad con BD existente.
    private static string ComputeMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

