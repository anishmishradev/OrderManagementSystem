
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Auth.DTOs;
using OrderManagement.Application.Interfaces.Services;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("getoken")]
    public IActionResult GetToken([FromBody] LoginRequest request)
    {
        var username = _configuration["Auth:Username"];
        var password = _configuration["Auth:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogError("Authentication configuration is missing.");
            return StatusCode(500, "Authentication is not configured.");
        }

        if (request.Username != username ||
            request.Password != password)
        {
            _logger.LogWarning(
                "Invalid login attempt for user {Username}",
                request.Username);

            return Unauthorized("Invalid username or password.");
        }

        var token = _jwtTokenService.GenerateToken(username, "Admin");

        _logger.LogInformation(
            "JWT token returned successfully for user {Username}",
            username);

        return Ok(new
        {
            AccessToken = token,
            TokenType = "Bearer"
        });
    }
}
