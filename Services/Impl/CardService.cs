using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Services.Impl;

public class CardService(AppDbContext context, IOptions<GameSettings> gameSettings) : ICardService
{
    private readonly AppDbContext _context = context;
    private readonly List<string> _starterCardNames = gameSettings.Value.StarterCardNames;

    public async Task<List<Card>> GetInitialCards()
    {
        List<Card> cardTemplates = await _context
            .Cards.Where(c => _starterCardNames.Contains(c.Name))
            .ToListAsync();

        return cardTemplates.Count != _starterCardNames.Count
            ? throw new CardsMissingException()
            : cardTemplates;
    }

    public async Task<Card> GetCardDetails(Guid cardId)
    {
        Card? card = await _context
            .Cards.FirstOrDefaultAsync(c => c.Id == cardId) ?? throw new CardNotFoundException();

        return card;
    }
}
