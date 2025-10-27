using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Services.Factories;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaService(IArenaTemplateService arenaTemplateService, IArenaEntityFactory attackEntityFactory) : IArenaService
    {
        private readonly IArenaTemplateService _arenaTemplateService = arenaTemplateService;
        private readonly IArenaEntityFactory _attackEntityFactory = attackEntityFactory;

        public async Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate arenaTemplate = await _arenaTemplateService.GetDefaultArenaTemplate();

            return new Arena(arenaTemplate, towers);
        }

        public ArenaEntity CreateEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y)
        {
            if (!arena.IsInsideBounds(x, y)) throw new InvalidSpawnPositionException(x, y);

            ArenaEntity entity = _attackEntityFactory.CreateEntity(player, card, x, y);
            arena.PlaceEntity(entity);

            return entity;
        }

        public void PlaceEntity(Arena arena, ArenaEntity entity)
        {
            arena.PlaceEntity(entity);
        }

        public void RemoveEntity(Arena arena, ArenaEntity entity)
        {
            arena.RemoveEntity(entity);
        }

        public double CalculateDistance(Positioned sourceEntity, Positioned targetEntity)
        {
            int dx = Math.Abs(targetEntity.X - sourceEntity.X);
            int dy = Math.Abs(targetEntity.Y - sourceEntity.Y);

            return Math.Max(dx, dy);
        }

        public IEnumerable<ArenaEntity> GetEnemiesInVision(Arena arena, TroopEntity troop)
        {
            double vision = (troop.PlayerCard.Card as TroopCard)!.VisionRange;

            foreach (var kvp in arena.Entities)
            {
                if (kvp.Key == troop.UserId) continue;

                foreach (var enemy in kvp.Value)
                {
                    double distance = CalculateDistance(troop, enemy);
                    if (distance <= vision) yield return enemy;
                }
            }
        }

        public Tower GetNearestEnemyTower(Arena arena, TroopEntity troop)
        {
            var enemyTowers = arena.Towers
                .Where(kvp => kvp.Key != troop.UserId)
                .SelectMany(kvp => kvp.Value);

            return enemyTowers
                .OrderBy(t => CalculateDistance(troop, t))
                .FirstOrDefault() ?? throw new EnemyTowersNotFoundException();
        }

        public void KillPositioned(Arena arena, Positioned positioned)
        {
            if (positioned is ArenaEntity arenaEntity)
            {
                arena.KillArenaEntity(arenaEntity);
            }
            else if (positioned is Tower tower)
            {
                arena.KillTower(tower);
            }
        }
    }
}