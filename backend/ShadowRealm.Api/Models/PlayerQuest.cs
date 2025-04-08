namespace ShadowRealm.Api.Models;

public class PlayerQuest
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    public int QuestId { get; set; }
    public Quest? Quest { get; set; } = null!;
}