using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IAuthService
    {
        Task<User> RegisterUser(string username, string email, string password);
        Task<User> LoginUser(string email, string password);
    }
}