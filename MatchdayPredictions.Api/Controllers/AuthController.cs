using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    private readonly IMetricsProvider _metrics;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IOptions<JwtSettings> jwtOptions,
        IUserRepository userRepository,
        IMetricsProvider metrics,
        ILogger<AuthController> logger)
    {
        _jwtSettings = jwtOptions.Value;
        _userRepository = userRepository;
        _metrics = metrics;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();

                return Unauthorized(new
                {
                    error = "Invalid username or password."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.TokenHours),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _metrics.IncrementRequestSuccess();

            return Ok(new { token = tokenString });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for username {Username}", request.Username);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred during login.");
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}

