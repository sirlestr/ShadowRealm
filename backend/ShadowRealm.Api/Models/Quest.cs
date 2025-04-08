namespace ShadowRealm.Api.Models;

public class Quest
{
    public int Id { get; set; }
    
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int RevardXP { get; set; }
}