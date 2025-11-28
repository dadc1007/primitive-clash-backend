using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUser(string oid, string email);
    }
}