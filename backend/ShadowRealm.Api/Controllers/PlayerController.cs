using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShadowRealm.Api.Models.Players;
using ShadowRealm.Api.Models.Responses;
using ShadowRealm.Api.Services;

namespace ShadowRealm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerController : BaseApiController
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IPlayerService _playerService;

    public PlayerController(
        ILogger<PlayerController> logger,
        IPlayerService playerService)
    {
        _logger = logger;
        _playerService = playerService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PlayerInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerInfoResponse>> Me()
    {
        var playerIdResult = GetCurrentPlayerIdOrUnauthorized();
        if (playerIdResult.Result is UnauthorizedObjectResult)
            return playerIdResult.Result;
        
        var playerId = playerIdResult.Value;

        var player = await _playerService.GetPlayerInfoAsync(playerId);
        if (player is null)
        {
            _logger.LogWarning("Player {PlayerId} not found", playerId);
            return NotFound("Player not found");
        }

        return Ok(player);
    }

    [HttpPost("save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save([FromBody] PlayerSaveRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var playerIdResult = GetCurrentPlayerIdOrUnauthorized();
        if (playerIdResult.Result is UnauthorizedObjectResult)
            return playerIdResult.Result;
        
        var playerId = playerIdResult.Value;

        var result = await _playerService.SavePositionAsync(
            playerId, request.PosX, request.PosY, request.PosZ);

        if (!result)
        {
            _logger.LogWarning("Failed to save position for player {PlayerId}", playerId);
            return NotFound("Player not found");
        }

        _logger.LogInformation("Position saved for playerId {PlayerId}: ({X}, {Y}, {Z})", 
            playerId, request.PosX, request.PosY, request.PosZ);
        
        return Ok(new { message = "Player position saved" });
    }

    [HttpGet("state")]
    [ProducesResponseType(typeof(PlayerStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerStateResponse>> GetState()
    {
        var playerIdResult = GetCurrentPlayerIdOrUnauthorized();
        if (playerIdResult.Result is UnauthorizedObjectResult)
            return playerIdResult.Result;
        
        var playerId = playerIdResult.Value;

        var state = await _playerService.GetPlayerStateAsync(playerId);
        if (state is null)
        {
            _logger.LogWarning("Player {PlayerId} not found", playerId);
            return NotFound("Player not found");
        }

        return Ok(state);
    }
}