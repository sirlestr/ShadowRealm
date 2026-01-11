namespace ShadowRealm.Api.Models.Responses;

public class QuestCompletionResponse
{
    public string Message { get; set; } = string.Empty;
    public int ExperienceGained { get; set; }
    public int TotalXP { get; set; }
}