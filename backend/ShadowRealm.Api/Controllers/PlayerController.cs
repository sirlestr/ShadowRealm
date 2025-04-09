using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Models.Players;

namespace ShadowRealm.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlayerController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return Unauthorized("User not authenticated");
        
        var player = _db.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is null)
            return NotFound("Player not found");

        return Ok(new
        {
            player.Id,
            player.Username,
            player.Level,
            player.Experience,
        });
    }



    [Authorize]
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] PlayerSaveRequest request)
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return Unauthorized();
        
        var player = await _db.Players.FindAsync(playerId);
        if (player is null)
            return NotFound();
        
        //save position
        
        player.PosX = request.PosX;
        player.PosY = request.PosY;
        player.PosZ = request.PosZ;

        _db.SaveChangesAsync();
        return Ok(new {message = "Player position saved"});
    }


    [Authorize]
    [HttpGet("state")]
    public async Task<ActionResult<PlayerStateResponse>> GetState()
    { 
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return Unauthorized();
        
        var player = await _db.Players.FindAsync(playerId);
        if (player is null)
            return NotFound();

        var state = new PlayerStateResponse
        {
            PosX = player.PosX,
            PosY = player.PosY,
            PosZ = player.PosZ,
            Level = player.Level,
            Experience = player.Experience
        };
        return Ok(state);

    }
    
    
}