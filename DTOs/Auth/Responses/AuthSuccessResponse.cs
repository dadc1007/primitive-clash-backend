namespace PrimitiveClash.Backend.DTOs.Auth.Responses
{
    public record AuthSuccessResponse(
        Guid UserId,
        string Username,
        string Email,
        int Gold,
        int Gems,
        int Level,
        int Trophies
    );
}
