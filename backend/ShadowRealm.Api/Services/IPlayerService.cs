using ShadowRealm.Api.Models.Players;
using ShadowRealm.Api.Models.Responses;

namespace ShadowRealm.Api.Services;

    public interface IPlayerService
    {
        Task<PlayerInfoResponse?> GetPlayerInfoAsync(int playerId);
        Task<PlayerStateResponse?> GetPlayerStateAsync(int playerId);
        Task<bool> SavePositionAsync(int playerId, float x, float y, float z);
    }
