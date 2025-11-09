using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository _repository;
    private readonly IMetricsProvider _metrics;
    private readonly ILogger<MatchesController> _logger;

    public MatchesController(
        IMatchRepository repository,
        IMetricsProvider metrics,
        ILogger<MatchesController> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(Match), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int matchId)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var matchItem = await _repository.GetByIdAsync(matchId);

            if (matchItem == null)
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();
                return NotFound();
            }

            _metrics.IncrementRequestSuccess();
            return Ok(matchItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving match {MatchId}", matchId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while retrieving the match."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Match>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByLeague([FromQuery] int leagueId)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var matches = await _repository.GetByLeagueAsync(leagueId);

            _metrics.IncrementRequestSuccess();
            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving matches for league {LeagueId}", leagueId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while retrieving matches."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            await _repository.CreateAsync(request);

            _metrics.IncrementRequestSuccess();

            return CreatedAtAction(nameof(GetById), new { matchId = 0 }, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating match");

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while creating the match."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}
