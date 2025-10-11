using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(string username, string email, string password);
        Task<User> loginUser(string username, string password);
    }
}