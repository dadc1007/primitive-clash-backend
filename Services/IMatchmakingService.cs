namespace PrimitiveClash.Backend.Services
{
    public interface IMatchmakingService
    {
        Task EnqueuePlayer(Guid userId, string connectionId);
        void DequeuePlayer(Guid userId);
    }
}