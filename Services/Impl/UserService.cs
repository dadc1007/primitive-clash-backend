using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Models;
using PrimitiveClashBackend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IDeckService _deckService;

        public UserService(AppDbContext context, IDeckService deckService)
        {
            _context = context;
            _deckService = deckService;
        }

        public async Task<User> RegisterUser(string username, string email, string password)
        {
            User user = new()
            {
                Username = username,
                Email = email,
                PasswordHash = password
            };

            Deck deck = await _deckService.InitializeDeck(user.Id);
            user.Deck = deck;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}