using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PlayerStateService : IPlayerStateService
    {
        public PlayerState CreatePlayerState(Guid userId)
        {
            return new PlayerState(userId);
        }
    }
}