using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class PlayerStateService(IDeckService deckService, IUserService userService) : IPlayerStateService
    {
        private readonly IDeckService _deckService = deckService;
        private readonly IUserService _userService = userService;
        private static readonly Random Rng = new();

        public async Task<PlayerState> CreatePlayerState(Guid userId)
        {
            string username = await _userService.GetUserName(userId);
            Deck deck = await _deckService.GetDeckByUserId(userId);
            List<PlayerCard> initialCards = Shuffle(deck);

            return new PlayerState(userId, username, initialCards);
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