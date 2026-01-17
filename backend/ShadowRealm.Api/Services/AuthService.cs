using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Models;
using ShadowRealm.Api.Configuration;

namespace ShadowRealm.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<string> _hasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        TokenService tokenService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _hasher = new PasswordHasher<string>();
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        var existingPlayer = await _db.Players
            .FirstOrDefaultAsync(p => p.Username == username);
        
        if (existingPlayer != null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Username already taken"
            };
        }

        var player = new Player
        {
            Username = username,
            PasswordHash = _hasher.HashPassword(username, password),
            Level = 1,
            Experience = 0,
            PosX = 0,
            PosY = 0,
            PosZ = 0
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        _logger.LogInformation("New player registered: {Username} (ID: {PlayerId})", 
            username, player.Id);

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.Username == username);

        if (player == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Username}", username);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        var verificationResult = _hasher.VerifyHashedPassword(
            username, player.PasswordHash, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", username);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        var token = _tokenService.GenerateToken(player.Id, player.Username);

        _logger.LogInformation("User logged in: {Username} (ID: {PlayerId})", 
            username, player.Id);

        return new AuthResult
        {
            Success = true,
            Token = token
        };
    }
}