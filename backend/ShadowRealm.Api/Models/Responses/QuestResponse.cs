namespace ShadowRealm.Api.Models.Responses;

public class QuestResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RewardXP { get; set; }
}