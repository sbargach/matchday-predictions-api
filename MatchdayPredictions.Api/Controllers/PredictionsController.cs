using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionRepository _repository;
    private readonly IMetricsProvider _metrics;
    private readonly ILogger<PredictionsController> _logger;

    public PredictionsController(
        IPredictionRepository repository,
        IMetricsProvider metrics,
        ILogger<PredictionsController> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPrediction([FromBody] CreatePredictionRequest request)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            await _repository.AddPredictionAsync(request);
            _metrics.IncrementRequestSuccess();

            return CreatedAtAction(nameof(GetPrediction),
                new { matchId = request.MatchId, userId = request.UserId },
                request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding prediction for match {MatchId}", request.MatchId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while processing the request."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPrediction([FromRoute] int matchId, [FromQuery] int userId)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var prediction = await _repository.GetPredictionAsync(matchId, userId);

            if (prediction == null)
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();
                return NotFound();
            }

            _metrics.IncrementRequestSuccess();
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving prediction for match {MatchId}", matchId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred while retrieving the prediction."
            });
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}
