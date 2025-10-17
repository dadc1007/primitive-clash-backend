using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class DeckService(AppDbContext context, IPlayerCardService playerCardService, IOptions<GameSettings> gameSettings) : IDeckService
    {
        private readonly AppDbContext _context = context;
        private readonly IPlayerCardService _playerCardService = playerCardService;
        private readonly int _maxSizeDeck = gameSettings.Value.MaxDeckSize;

        public async Task<Deck> InitializeDeck(Guid userId)
        {
            Deck deck = new(_maxSizeDeck)
            {
                UserId = userId
            };
            List<PlayerCard> initialPlayerCards = await _playerCardService.CreateStarterCards(userId, deck.Id);

            if (initialPlayerCards.Count != _maxSizeDeck)
            {
                throw new InvalidDeckSizeException(_maxSizeDeck);
            }

            deck.PlayerCards = initialPlayerCards;

            _context.Decks.Add(deck);

            return deck;
        }

        public async Task<Deck> GetDeckByUserId(Guid userId)
        {
            Deck? deck = await _context.Decks
                .Where(d => d.UserId == userId)
                .Include(d => d.PlayerCards)
                .ThenInclude(pc => pc.Card)
                .FirstOrDefaultAsync() ?? throw new DeckNotFoundException(userId);
            return deck;
        }
    }
}