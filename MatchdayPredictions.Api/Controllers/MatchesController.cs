using MatchdayPredictions.Api.DataAccess.Repository;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository _repository;

    public MatchesController(IMatchRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a match by its unique identifier.
    /// </summary>
    [HttpGet("{matchId:int}")]
    [ProducesResponseType(typeof(Match), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int matchId)
    {
        var matchItem = await _repository.GetByIdAsync(matchId);
        return matchItem == null ? NotFound() : Ok(matchItem);
    }

    /// <summary>
    /// Retrieves all matches within a league.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Match>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLeague([FromQuery] int leagueId)
    {
        var matches = await _repository.GetByLeagueAsync(leagueId);
        return Ok(matches);
    }

    /// <summary>
    /// Creates a new match. Requires authentication.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request)
    {
        await _repository.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { matchId = 0 }, request);
    }
}
