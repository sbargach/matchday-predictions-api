using MatchdayPredictions.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

/// <summary>
/// Controller responsible for handling match predictions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class PredictionsController : ControllerBase
{
    /// <summary>
    /// Returns an example match prediction for demonstration purposes.
    /// </summary>
    [HttpGet("prediction")]
    [ProducesResponseType(typeof(MatchPrediction), StatusCodes.Status200OK)]
    public IActionResult GetExamplePrediction()
    {
        var demo = new MatchPrediction
        {
            MatchId = 1,
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            HomeGoals = null,
            AwayGoals = null,
            KickoffUtc = DateTime.UtcNow.AddDays(1)
        };

        return Ok(demo);
    }
}