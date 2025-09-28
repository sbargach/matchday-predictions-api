using MatchdayPredictions.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

/// <summary>Handles match predictions (currently placeholder only).</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PredictionsController : ControllerBase
{
    /// <summary>Adds a prediction for a specific match (placeholder).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status201Created)]
    public IActionResult AddPrediction([FromBody] CreatePredictionRequest request)
    {
        // Placeholder: 
        var prediction = new MatchPrediction
        {
            MatchId = request.MatchId,
            UserId = request.UserId,
            HomeTeam = request.HomeTeam,
            AwayTeam = request.AwayTeam,
            HomeGoals = request.HomeGoals,
            AwayGoals = request.AwayGoals,
            KickoffUtc = request.KickoffUtc
        };

        return CreatedAtAction(nameof(GetPrediction),
            new { matchId = prediction.MatchId, userId = prediction.UserId }, prediction);
    }

    /// <summary>Gets a prediction for a specific match by a given user (placeholder).</summary>
    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPrediction([FromRoute] int matchId, [FromQuery] int userId)
    {
        // Placeholder
        var demo = new MatchPrediction
        {
            MatchId = matchId,
            UserId = userId,
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            HomeGoals = 2,
            AwayGoals = 1,
            KickoffUtc = DateTime.UtcNow.AddDays(1)
        };

        return Ok(demo);
    }
}
