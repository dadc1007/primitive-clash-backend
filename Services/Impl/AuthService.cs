using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class AuthService(AppDbContext context, IUserService userService) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly IUserService _userService = userService;

        public async Task<User> RegisterUser(string username, string email, string password)
        {
            return await _userService.RegisterUser(username, email, password);
        }

        public async Task<User> LoginUser(string email, string password)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.PasswordHash != password)
            {
                throw new InvalidCredentialsException();
            }

            return user;
        }
    }
}