using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IPlayerStateService
    {
        PlayerState CreatePlayerState(Guid userId);
    }
}
