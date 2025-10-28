using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IDeckService
    {
        Task<Deck> InitializeDeck(Guid userId);
        Task<Deck> GetDeckByUserId(Guid userId);
    }
}