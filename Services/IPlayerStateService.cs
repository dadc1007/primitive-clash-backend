using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IPlayerStateService
    {
        Task<PlayerState> CreatePlayerState(Guid userId);
    }
}
