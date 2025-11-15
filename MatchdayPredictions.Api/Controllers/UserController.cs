using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IMetricsProvider _metrics;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repository, IMetricsProvider metrics, ILogger<UsersController> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            await _repository.CreateUserAsync(request);

            _metrics.IncrementRequestSuccess();
            return CreatedAtAction(nameof(GetSelf), new { }, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSelf()
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _repository.GetUserByIdAsync(currentUserId);

            if (user == null)
            {
                _metrics.IncrementClientError();
                return NotFound();
            }

            _metrics.IncrementRequestSuccess();
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve current user");
            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}
