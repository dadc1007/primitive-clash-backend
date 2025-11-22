using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PlayerCardService(AppDbContext context, ICardService cardService)
        : IPlayerCardService
    {
        private readonly AppDbContext _context = context;
        private readonly ICardService _cardService = cardService;

        public async Task<List<PlayerCard>> CreateStarterCards(Guid userId, Guid deckId)
        {
            List<Card> cardTemplates = await _cardService.GetInitialCards();
            List<PlayerCard> initialPlayerCards = [];

            initialPlayerCards.AddRange(
                cardTemplates.Select(cardTemplate => new PlayerCard()
                {
                    CardId = cardTemplate.Id,
                    Card = cardTemplate,
                    DeckId = deckId,
                    UserId = userId,
                })
            );

            _context.PlayerCards.AddRange(initialPlayerCards);

            return initialPlayerCards;
        }
    }
}
