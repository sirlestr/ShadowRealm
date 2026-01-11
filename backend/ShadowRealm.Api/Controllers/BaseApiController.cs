using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShadowRealm.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected int? GetCurrentPlayerId()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return null;
        
        return playerId;
    }

    protected ActionResult<int> GetCurrentPlayerIdOrUnauthorized()
    {
        var playerId = GetCurrentPlayerId();
        if (playerId is null)
            return Unauthorized("User not authenticated");
        
        return playerId.Value;
    }
}