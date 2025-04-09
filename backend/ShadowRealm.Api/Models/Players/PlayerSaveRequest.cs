using System.ComponentModel.DataAnnotations;
namespace ShadowRealm.Api.Models.Players;

public class PlayerSaveRequest
{
    [Required]
    public float PosX { get; set; }
    
    [Required]
    public float PosY { get; set; }
    
    [Required]
    public float PosZ { get; set; }
}