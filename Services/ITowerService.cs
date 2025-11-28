using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface ITowerService
    {
        Task<Tower> CreateLeaderTower(Guid playerStateId);
        Task<Tower> CreateGuardianTower(Guid playerStateId);
        Task<Dictionary<Guid, List<Tower>>> CreateAllGameTowers(Guid player1StateId, Guid player2StateId);
    }
}