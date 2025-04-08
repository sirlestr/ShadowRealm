using System.ComponentModel.DataAnnotations;

namespace ShadowRealm.Api.Models;

public class Player
{
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string PasswordHash { get; set; } = null!;
    
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }

    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    
    public List<PlayerQuest>? CompletedQuest { get; set; } = new();
    
    
}