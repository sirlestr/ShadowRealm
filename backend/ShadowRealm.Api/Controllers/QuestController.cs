using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Models;
using ShadowRealm.Api.Models.Responses;

namespace ShadowRealm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestController : ControllerBase
{
    private readonly ILogger<QuestController> _logger;
    private readonly AppDbContext _db;
    
    public QuestController(ILogger<QuestController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // [HttpGet]
    // public ActionResult<IEnumerable<Quest>> GetAll()
    // {
    //     var quests = _db.Quests.ToList();
    //     return Ok(quests);
    // }
    
    [Authorize]
    [HttpGet]
    public ActionResult<IEnumerable<Quest>> GetAvailableforPlayer()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return Unauthorized();
        
        // Seznam ID již splněných questů
        var completedQuestIds = _db.PlayerQuests
            .Where(pq => pq.PlayerId == playerId)
            .Select(pq => pq.QuestId)
            .ToList();
        
        // Dostupné questy = ty, které hráč ještě nesplnil
        var quests = _db.Quests
            .Where(q => !completedQuestIds.Contains(q.Id))
            .Select(q => new QuestResponse
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                RewardXP = q.RevardXP
            })
            .ToList();

        return Ok(quests);
    }

    [Authorize]
    [HttpPost("complete/{id}")]
    public async Task<IActionResult> CompleteQuest(int id)
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (playerIdClaim is null || !int.TryParse(playerIdClaim.Value, out var playerId))
            return Unauthorized();
        
        var player = await _db.Players.FindAsync(playerId);
        if (player == null)
            return NotFound("Player not found");
        
        var quest = await _db.Quests.FindAsync(id);
        if (quest == null)
            return NotFound("Quest not found");
        
        var alreadyCompleted = _db.PlayerQuests.Any(pq => pq.PlayerId == playerId && pq.QuestId == id);
        if (alreadyCompleted)
            return BadRequest("Quest already completed");
        
        // Přidání quest do seznamu splněných questů hráče
        var playerQuest = new PlayerQuest
        {
            PlayerId = playerId,
            QuestId = id,
        };
        
        _db.PlayerQuests.Add(playerQuest);
        
        // Přidání XP hráči
        player.Experience += quest.RevardXP;
        
        await _db.SaveChangesAsync();
        
        return Ok(new {
            message = "Quest completed",
            experienceGained = quest.RevardXP,
            totalXp=player.Experience });
    }
    
}