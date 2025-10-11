using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IPlayerCardService
    {
        Task<List<PlayerCard>> CreateStarterCards(Guid userId, Guid deckId);
    }
}