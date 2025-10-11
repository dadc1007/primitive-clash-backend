using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class DeckService : IDeckService
    {
        private readonly AppDbContext _context;
        private readonly IPlayerCardService _playerCardService;
        private readonly int _maxSizeDeck;

        public DeckService(AppDbContext context, IPlayerCardService playerCardService, IOptions<GameSettings> gameSettings)
        {
            _context = context;
            _playerCardService = playerCardService;
            _maxSizeDeck = gameSettings.Value.MaxDeckSize;
        }

        public async Task<Deck> InitializeDeck(Guid userId)
        {
            Deck deck = new(_maxSizeDeck);
            List<PlayerCard> initialPlayerCards = await _playerCardService.CreateStarterCards(userId, deck.Id);

            if (initialPlayerCards.Count != _maxSizeDeck)
            {
                throw new InvalidDeckSizeException(_maxSizeDeck);
            }

            deck.PlayerCards = initialPlayerCards;

            _context.Decks.Add(deck);

            return deck;
        }
    }
}