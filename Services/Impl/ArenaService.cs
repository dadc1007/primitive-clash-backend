using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Services.Factories;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaService(IArenaTemplateService arenaTemplateService, IAttackEntityFactory attackEntityFactory) : IArenaService
    {
        private readonly IArenaTemplateService _arenaTemplateService = arenaTemplateService;
        private readonly IAttackEntityFactory _attackEntityFactory = attackEntityFactory;

        public async Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate arenaTemplate = await _arenaTemplateService.GetDefaultArenaTemplate();

            return new Arena(arenaTemplate, towers);
        }

        public AttackEntity CreateEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y)
        {
            if (!arena.IsInsideBounds(x, y)) throw new InvalidSpawnPositionException(x, y);

            AttackEntity entity = _attackEntityFactory.CreateEntity(player, card, x, y);
            arena.PlaceEntity(entity);

            return entity;
        }

        public void PlaceEntity(Arena arena, AttackEntity entity)
        {
            arena.PlaceEntity(entity);
        }

        public void RemoveEntity(Arena arena, AttackEntity entity)
        {
            arena.RemoveEntity(entity);
        }

        public double CalculateDistance(AttackEntity sourceEntity, AttackEntity targetEntity)
        {
            return Math.Sqrt(Math.Pow(targetEntity.PosX - sourceEntity.PosX, 2) + Math.Pow(targetEntity.PosY - sourceEntity.PosY, 2));
        }

        public IEnumerable<AttackEntity> GetEnemiesInVision(Arena arena, TroopEntity troop)
        {
            double vision = (troop.Card.Card as TroopCard)!.VisionRange;

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
    }
}