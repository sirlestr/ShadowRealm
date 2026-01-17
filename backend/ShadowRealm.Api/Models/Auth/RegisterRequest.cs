using System.ComponentModel.DataAnnotations;
namespace ShadowRealm.Api.Models.Auth;

public class RegisterRequest
{
    [Required]
    [MinLength(3),MaxLength(20)]
    public string Username { get; set; }
    
    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Heslo musí obsahovat velké písmeno, malé písmeno a číslo")]
    public string Password { get; set; }
}