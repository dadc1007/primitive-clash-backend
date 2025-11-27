using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Services;

public interface ICardService
{
    Task<List<Card>> GetInitialCards();
    Task<Card> GetCardDetails(Guid cardId);     
}