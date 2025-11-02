using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user details to insert.</param>
    /// <returns>A 201 Created response if successful.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        await _repository.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { userId = 0 }, request);
    }

    /// <summary>
    /// Retrieves a user by their unique ID.
    /// </summary>
    /// <param name="userId">The user ID to retrieve.</param>
    /// <returns>The user if found, otherwise 404.</returns>
    [HttpGet("{userId:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int userId)
    {
        var user = await _repository.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
