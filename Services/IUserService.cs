using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUser(string oid, string email);
        Task<string> GetUserName(Guid userId);
        Task UpdateUserMatchId(Guid userId, Guid? matchId);
        Task<Guid?> GetMatchId(Guid userId);
    }
}