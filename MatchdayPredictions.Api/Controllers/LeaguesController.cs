using MatchdayPredictions.Api.DataAccess.Repository;
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
public class LeaguesController : ControllerBase
{
    private readonly ILeagueRepository _repository;
    private readonly IMetricsProvider _metrics;
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(
        ILeagueRepository repository,
        IMetricsProvider metrics,
        ILogger<LeaguesController> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<League>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var leagues = await _repository.GetAllAsync();
            _metrics.IncrementRequestSuccess();
            return Ok(leagues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all leagues.");

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while retrieving the leagues."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    [HttpGet("{leagueId:int}")]
    [ProducesResponseType(typeof(League), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int leagueId)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var league = await _repository.GetByIdAsync(leagueId);

            if (league == null)
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();
                return NotFound();
            }

            _metrics.IncrementRequestSuccess();
            return Ok(league);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving league {LeagueId}", leagueId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while retrieving the league."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}
