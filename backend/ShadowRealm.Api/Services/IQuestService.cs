using ShadowRealm.Api.Models.Responses;

namespace ShadowRealm.Api.Services;

public interface IQuestService
{
    Task<IEnumerable<QuestResponse>> GetAvailableQuestsAsync(int playerId);
    Task<QuestCompletionResult> CompleteQuestAsync(int playerId, int questId);
}

public class QuestCompletionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExperienceGained { get; set; }
    public int TotalExperience { get; set; }
}