using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

/// <summary>
/// API controller for managing match predictions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionRepository _repository;

    public PredictionsController(IPredictionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Adds or updates a match prediction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddPrediction([FromBody] CreatePredictionRequest request)
    {
        await _repository.AddPredictionAsync(request);

        return CreatedAtAction(nameof(GetPrediction),
            new { matchId = request.MatchId, userId = request.UserId },
            request);
    }

    /// <summary>
    /// Gets a match prediction by match ID and user ID.
    /// </summary>
    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrediction([FromRoute] int matchId, [FromQuery] int userId)
    {
        var prediction = await _repository.GetPredictionAsync(matchId, userId);

        if (prediction == null)
            return NotFound();

        return Ok(prediction);
    }
}
