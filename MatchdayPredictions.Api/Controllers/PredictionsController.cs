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
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
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

    /// <summary>
    /// Creates or updates a prediction. User can ONLY submit predictions for themselves.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPrediction([FromBody] CreatePredictionRequest request)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Enforce: user can only create THEIR own predictions
            if (currentUserId != request.UserId)
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();
                return StatusCode(StatusCodes.Status403Forbidden,
                    ErrorResponse.FromMessage("You can only create predictions for your own user."));
            }

            var result = await _repository.AddPredictionAsync(request);
            switch (result)
            {
                case PredictionWriteResult.Created:
                    _metrics.IncrementRequestSuccess();
                    return CreatedAtAction(nameof(GetPrediction),
                        new { matchId = request.MatchId, userId = request.UserId }, request);
                case PredictionWriteResult.Updated:
                    _metrics.IncrementRequestSuccess();
                    return Ok(request);
                case PredictionWriteResult.MatchNotFound:
                    _metrics.IncrementClientError();
                    _metrics.IncrementRequestFailure();
                    return NotFound(ErrorResponse.FromMessage("Match not found."));
                case PredictionWriteResult.MatchStarted:
                    _metrics.IncrementClientError();
                    _metrics.IncrementRequestFailure();
                    return Conflict(ErrorResponse.FromMessage("Predictions close at kickoff."));
                case PredictionWriteResult.NotLeagueMember:
                    _metrics.IncrementClientError();
                    _metrics.IncrementRequestFailure();
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ErrorResponse.FromMessage("You must be a league member to predict this match."));
                default:
                    throw new InvalidOperationException($"Unexpected prediction result: {result}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while adding prediction for match {MatchId}", request.MatchId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorResponse.FromMessage("An unexpected error occurred while processing the request."));
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Retrieves a match prediction. User can ONLY retrieve their OWN predictions.
    /// </summary>
    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPrediction([FromRoute] int matchId, [FromQuery] int userId)
    {
        var sw = Stopwatch.StartNew();
        _metrics.IncrementRequest();

        try
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Enforce: user can only retrieve THEIR own predictions
            if (currentUserId != userId)
            {
                _metrics.IncrementClientError();
                return StatusCode(StatusCodes.Status403Forbidden,
                    ErrorResponse.FromMessage("You can only view your own predictions."));
            }

            var prediction = await _repository.GetPredictionAsync(matchId, userId);

            if (prediction == null)
            {
                _metrics.IncrementClientError();
                _metrics.IncrementRequestFailure();
                return NotFound(ErrorResponse.FromMessage("Prediction not found."));
            }

            _metrics.IncrementRequestSuccess();
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving prediction for match {MatchId}", matchId);

            _metrics.IncrementServerError();
            _metrics.IncrementRequestFailure();

            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorResponse.FromMessage("An unexpected error occurred while retrieving the prediction."));
        }
        finally
        {
            _metrics.RecordRequestDuration(sw.Elapsed.TotalMilliseconds);
        }
    }
}
