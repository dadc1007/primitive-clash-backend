using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class UserService(AppDbContext context, IDeckService deckService) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IDeckService _deckService = deckService;

        public async Task<User> GetOrCreateUser(string oid, string email)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == oid);

            if (user != null) return user;

            user = new User
            {
                Id = Guid.Parse(oid),
                Username = email.Split("@")[0],
                Email = email,
            };

            Deck deck = await _deckService.InitializeDeck(user.Id);
            user.Deck = deck;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string> GetUserName(Guid userId)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException(userId);

            return user.Username;
        }

        public async Task UpdateUserMatchId(Guid userId, Guid? matchId)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException(userId);
            user.MatchId = matchId;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid?> GetMatchId(Guid userId)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new UserNotFoundException(userId);
            return user.MatchId;
        }
    }
}