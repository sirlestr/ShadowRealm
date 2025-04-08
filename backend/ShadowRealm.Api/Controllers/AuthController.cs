 using Microsoft.AspNetCore.Mvc;
 using Microsoft.AspNetCore.Identity;
 using Microsoft.AspNetCore.Identity.Data;
 using Microsoft.EntityFrameworkCore;
 using ShadowRealm.Api.Data;
 using ShadowRealm.Api.Models;

 namespace ShadowRealm.Api.Controllers;

 public class AuthController : ControllerBase
 {
  private readonly AppDbContext _db;
  private readonly TokenService _tokenService;
  private readonly PasswordHasher<string> _hasher = new();

  public AuthController(AppDbContext db, TokenService tokenService)
  {
   _db = db;
   _tokenService = tokenService;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterRequest request)
  {
   if (_db.Players.Any(p => p.Username == request.Email))
    return BadRequest("Username already taken");

   var player = new Player
   {
    Username = request.Email,
    PasswordHash = _hasher.HashPassword(request.Email, request.Password),
   };
   
   _db.Players.Add(player);
   await _db.SaveChangesAsync();
   return Ok();
  }


  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginRequest request)
  {
   var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == request.Email);
   if(player == null)
    return Unauthorized("Invalid username");
   
   var result = _hasher.VerifyHashedPassword(request.Email, player.PasswordHash, request.Password);
   if(result == PasswordVerificationResult.Failed)
    return BadRequest("Invalid password");

   var token = _tokenService.GenerateToken(player.Id, player.Username);
   return Ok(new { token });
  }

 }