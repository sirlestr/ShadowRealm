using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Data;
using ShadowRealm.Api.Models.Players;
using ShadowRealm.Api.Models.Responses;

namespace ShadowRealm.Api.Services;

public class PlayerService : IPlayerService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(AppDbContext db, ILogger<PlayerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PlayerInfoResponse?> GetPlayerInfoAsync(int playerId)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return null;

        return new PlayerInfoResponse
        {
            Id = player.Id,
            Username = player.Username,
            Level = player.Level,
            Experience = player.Experience
        };
    }

    public async Task<PlayerStateResponse?> GetPlayerStateAsync(int playerId)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return null;

        return new PlayerStateResponse
        {
            PosX = player.PosX,
            PosY = player.PosY,
            PosZ = player.PosZ,
            Level = player.Level,
            Experience = player.Experience
        };
    }

    public async Task<bool> SavePositionAsync(int playerId, float x, float y, float z)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player == null) return false;

        player.PosX = x;
        player.PosY = y;
        player.PosZ = z;

        await _db.SaveChangesAsync();
        return true;
    }
}