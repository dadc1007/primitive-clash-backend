using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IDeckService
    {
        Task<Deck> InitializeDeck(Guid userId);
    }
}