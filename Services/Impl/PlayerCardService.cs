using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PlayerCardService : IPlayerCardService
    {
        private readonly AppDbContext _context;

        private readonly List<string> _starterCardNames;

        public PlayerCardService(AppDbContext context, IOptions<GameSettings> gameSettings)
        {
            _context = context;
            _starterCardNames = gameSettings.Value.StarterCardNames;
        }

        public async Task<List<PlayerCard>> CreateStarterCards(Guid userId, Guid deckId)
        {
            List<Card> cardTemplates = await _context.Cards
                .Where(c => _starterCardNames.Contains(c.Name))
                .ToListAsync();

            if (cardTemplates.Count != _starterCardNames.Count)
            {
                throw new CardsMissingException();
            }

            List<PlayerCard> initialPlayerCards = [];

            foreach (Card cardTemplate in cardTemplates)
            {
                PlayerCard playerCard = new()
                {
                    CardId = cardTemplate.Id,
                    Card = cardTemplate,
                    DeckId = deckId,
                    UserId = userId,
                };

                initialPlayerCards.Add(playerCard);
            }

            _context.PlayerCards.AddRange(initialPlayerCards);

            return initialPlayerCards;
        }
    }
}
