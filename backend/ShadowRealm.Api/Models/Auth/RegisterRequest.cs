using System.ComponentModel.DataAnnotations;
namespace ShadowRealm.Api.Models.Auth;

public class RegisterRequest
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}