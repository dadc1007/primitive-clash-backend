using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
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
            if (await UsernameExists(username))
            {
                throw new UsernameExistsException(username);
            }

            if (await EmailExists(email))
            {
                throw new EmailExistsException(email);
            }

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

        public async Task<User> loginUser(string username, string password)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.PasswordHash != password)
            {
                throw new InvalidCredentialsException();
            }

            return user;
        }

        private async Task<bool> UsernameExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        private async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}