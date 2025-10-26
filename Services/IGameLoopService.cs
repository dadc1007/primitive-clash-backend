namespace PrimitiveClash.Backend.Services
{
    public interface IGameLoopService
    {
        void StartGameLoop(Guid sessionId);
        void StopGameLoop(Guid sessionId);
        Task ProcessTick();
    }
}