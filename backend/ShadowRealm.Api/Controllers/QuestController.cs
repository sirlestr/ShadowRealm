using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShadowRealm.Api.Models.Responses;
using ShadowRealm.Api.Services;

namespace ShadowRealm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestController : BaseApiController
{
    private readonly ILogger<QuestController> _logger;
    private readonly IQuestService _questService;

    public QuestController(
        ILogger<QuestController> logger,
        IQuestService questService)
    {
        _logger = logger;
        _questService = questService;
    }

    /// <summary>
    /// Získá seznam dostupných questů pro aktuálního hráče
    /// (tj. ty, které ještě nesplnil)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<QuestResponse>>> GetAvailableForPlayer()
    {
        var playerIdResult = GetCurrentPlayerIdOrUnauthorized();
        if (playerIdResult.Result is UnauthorizedObjectResult)
            return playerIdResult.Result;
        
        var playerId = playerIdResult.Value;

        var quests = await _questService.GetAvailableQuestsAsync(playerId);
        
        _logger.LogInformation("Retrieved {Count} available quests for player {PlayerId}", 
            quests.Count(), playerId);
        
        return Ok(quests);
    }

    /// <summary>
    /// Označí quest jako splněný a přidá hráči odměnu (XP)
    /// </summary>
    [HttpPost("complete/{id}")]
    [ProducesResponseType(typeof(QuestCompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestCompletionResponse>> CompleteQuest(int id)
    {
        var playerIdResult = GetCurrentPlayerIdOrUnauthorized();
        if (playerIdResult.Result is UnauthorizedObjectResult)
            return playerIdResult.Result;
        
        var playerId = playerIdResult.Value;

        var result = await _questService.CompleteQuestAsync(playerId, id);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to complete quest {QuestId} for player {PlayerId}: {Error}", 
                id, playerId, result.ErrorMessage);
            
            return result.ErrorMessage switch
            {
                "Player not found" => NotFound(result.ErrorMessage),
                "Quest not found" => NotFound(result.ErrorMessage),
                "Quest already completed" => BadRequest(result.ErrorMessage),
                _ => BadRequest(result.ErrorMessage)
            };
        }

        _logger.LogInformation("Player {PlayerId} completed quest {QuestId}, gained {XP} XP", 
            playerId, id, result.ExperienceGained);

        return Ok(new QuestCompletionResponse
        {
            Message = "Quest completed",
            ExperienceGained = result.ExperienceGained,
            TotalXP = result.TotalExperience
        });
    }
}