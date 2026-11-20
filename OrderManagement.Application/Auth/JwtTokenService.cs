
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Interfaces.Services;

namespace OrderManagement.Application.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public string GenerateToken(string username, string role)
    {
        var cacheKey = $"jwt_{username}_{role}";

        if (_cache.TryGetValue(cacheKey, out string? cachedToken)
            && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogInformation(
                "JWT token retrieved from cache for user {Username}",
                username);

            return cachedToken;
        }

        var expiryMinutes = _configuration.GetValue<int>(
            "Jwt:ExpiryMinutes", 30);

        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = CreateToken(username, role, expiry);

        _cache.Set(cacheKey, token, new DateTimeOffset(expiry));

        _logger.LogInformation(
            "JWT token generated and cached for user {Username}. Expires at {Expiry}",
            username,
            expiry);

        return token;
    }

    private string CreateToken(string username, string role, DateTime expiry)
    {
        var claims = GetClaims(username, role);

        var jwtToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: GetSigningCredentials());

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is missing.");

        if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            throw new InvalidOperationException(
                "JWT key must be at least 32 bytes.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        return new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);
    }

    private static Claim[] GetClaims(string username, string role)
    {
        return
        [
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        ];
    }
}
