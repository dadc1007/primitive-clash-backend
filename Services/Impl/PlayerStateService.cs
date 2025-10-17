using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PlayerStateService(IDeckService deckService) : IPlayerStateService
    {
        private readonly IDeckService _deckService = deckService;
        private static readonly Random Rng = new Random();
        
        public async Task<PlayerState> CreatePlayerState(Guid userId)
        {
            Deck deck = await _deckService.GetDeckByUserId(userId);
            List<PlayerCard> initialCards = Shuffle(deck);
            
            return new PlayerState(userId, initialCards);
        }

        private static List<PlayerCard> Shuffle(Deck deck)
        {
            List<PlayerCard> list = deck.PlayerCards.ToList();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                
                int k = Rng.Next(n + 1);
                
                (list[k], list[n]) = (list[n], list[k]);
            }
            
            return list;
        }
    }
}