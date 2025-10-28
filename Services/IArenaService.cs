using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IArenaService
    {
        Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers);
        ArenaEntity CreateEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y);
        List<ArenaEntity> GetEntities(Arena arena);
        List<Tower> GetTowers(Arena arena); 
        void PlaceEntity(Arena arena, ArenaEntity entity);
        void RemoveEntity(Arena arena, ArenaEntity entity);
        double CalculateDistance(Positioned sourceEntity, Positioned targetEntity);
        IEnumerable<ArenaEntity> GetEnemiesInVision(Arena arena, Positioned positioned);
        Tower GetNearestEnemyTower(Arena arena, TroopEntity troop);
        bool CanExecuteMovement(Arena arena, ArenaEntity troop, int x, int y);
        void KillPositioned(Arena arena, Positioned positioned);
        (int towersWinner, int towersLosser) GetNumberTowers(Arena arena, Guid winnerId, Guid losserId);
    }
}