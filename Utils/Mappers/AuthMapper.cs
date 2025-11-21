using PrimitiveClash.Backend.DTOs.Auth.Responses;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Utils.Mappers
{
    public static class AuthMapper
    {
        public static AuthSuccessResponse ToAuthSuccessResponse(this User user)
        {
            if (user == null)
            {
                throw new InvalidOperationException("User is null");
            }

            return new AuthSuccessResponse(
                user.Id,
                user.Username,
                user.Email,
                user.Gold,
                user.Gems,
                user.Level,
                user.Trophies
            );
        }
    }
}