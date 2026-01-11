 using Microsoft.AspNetCore.Mvc;
 using Microsoft.AspNetCore.Identity;
 using Microsoft.EntityFrameworkCore;
 using ShadowRealm.Api.Data;
 using ShadowRealm.Api.Models;
 using ShadowRealm.Api.Models.Auth;
 using ShadowRealm.Api.Models.Responses;

 namespace ShadowRealm.Api.Controllers;

 [Route("api/[controller]")]
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
  public async Task<IActionResult> Register([FromBody]RegisterRequest request)
  {
   if(!ModelState.IsValid)
    return BadRequest(ModelState);
   
   if (_db.Players.Any(p => p.Username == request.Username))
    return BadRequest("Username already taken");

   var player = new Player
   {
    Username = request.Username,
    PasswordHash = _hasher.HashPassword(request.Username, request.Password),
   };
   
   _db.Players.Add(player);
   await _db.SaveChangesAsync();
   return Ok();
  }


  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody]LoginRequest request)
  {
   //Console.WriteLine($"LoginRequest: Username = '{request.Username}', Password = '{request.Password}'");
   
   
   var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == request.Username);
   if(player == null)
    return Unauthorized("Invalid username");
   
   var result = _hasher.VerifyHashedPassword(request.Username, player.PasswordHash, request.Password);
   if(result == PasswordVerificationResult.Failed)
    return BadRequest("Invalid password");

   //var token = _tokenService.GenerateToken(player.Id, player.Username);
   LoginResponse response = new(){ Token = _tokenService.GenerateToken(player.Id,player.Username) };
   return Ok( response);
  }

 }