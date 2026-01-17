using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Models;
using ShadowRealm.Api.Models.Responses;

namespace ShadowRealm.Api.Services;

public class QuestService : IQuestService
{
    private readonly AppDbContext _db;
    private readonly ILogger<QuestService> _logger;

    public QuestService(AppDbContext db, ILogger<QuestService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<QuestResponse>> GetAvailableQuestsAsync(int playerId)
    {
        // Seznam ID již splněných questů
        var completedQuestIds = await _db.PlayerQuests
            .Where(pq => pq.PlayerId == playerId)
            .Select(pq => pq.QuestId)
            .ToListAsync();

        // Dostupné questy = ty, které hráč ještě nesplnil
        var quests = await _db.Quests
            .Where(q => !completedQuestIds.Contains(q.Id))
            .Select(q => new QuestResponse
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                RewardXP = q.RewardXP
            })
            .ToListAsync();

        _logger.LogDebug("Found {Count} available quests for player {PlayerId}", 
            quests.Count, playerId);

        return quests;
    }

    public async Task<QuestCompletionResult> CompleteQuestAsync(int playerId, int questId)
    {
        // Najdi hráče
        var player = await _db.Players.FindAsync(playerId);
        if (player == null)
        {
            return new QuestCompletionResult
            {
                Success = false,
                ErrorMessage = "Player not found"
            };
        }

        // Najdi quest
        var quest = await _db.Quests.FindAsync(questId);
        if (quest == null)
        {
            return new QuestCompletionResult
            {
                Success = false,
                ErrorMessage = "Quest not found"
            };
        }

        // Zkontroluj, zda quest již není splněn
        var alreadyCompleted = await _db.PlayerQuests
            .AnyAsync(pq => pq.PlayerId == playerId && pq.QuestId == questId);
        
        if (alreadyCompleted)
        {
            return new QuestCompletionResult
            {
                Success = false,
                ErrorMessage = "Quest already completed"
            };
        }

        // Přidej quest do seznamu splněných
        var playerQuest = new PlayerQuest
        {
            PlayerId = playerId,
            QuestId = questId
        };
        _db.PlayerQuests.Add(playerQuest);

        // Přidej XP hráči
        player.Experience += quest.RewardXP;

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Player {PlayerId} completed quest {QuestId} '{Title}' and gained {XP} XP", 
            playerId, questId, quest.Title, quest.RewardXP);

        return new QuestCompletionResult
        {
            Success = true,
            ExperienceGained = quest.RewardXP,
            TotalExperience = player.Experience
        };
    }
}