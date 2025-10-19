using MatchdayPredictions.Api.DataAccess.Repository;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueRepository _repository;

    public LeaguesController(ILeagueRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<League>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var leagues = await _repository.GetAllAsync();
        return Ok(leagues);
    }

    [HttpGet("{leagueId:int}")]
    [ProducesResponseType(typeof(League), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int leagueId)
    {
        var league = await _repository.GetByIdAsync(leagueId);
        if (league == null)
            return NotFound();

        return Ok(league);
    }
}
