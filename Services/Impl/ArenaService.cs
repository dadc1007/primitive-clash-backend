using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Services.Factories;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaService(
        IArenaTemplateService arenaTemplateService,
        IArenaEntityFactory attackEntityFactory
    ) : IArenaService
    {
        private readonly IArenaTemplateService _arenaTemplateService = arenaTemplateService;
        private readonly IArenaEntityFactory _attackEntityFactory = attackEntityFactory;

        public async Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate arenaTemplate = await _arenaTemplateService.GetDefaultArenaTemplate();

            return new Arena(arenaTemplate, towers);
        }

        public ArenaEntity CreateEntity(
            Arena arena,
            PlayerState player,
            PlayerCard card,
            int x,
            int y
        )
        {
            if (!arena.IsInsideBounds(x, y))
                throw new InvalidSpawnPositionException(x, y);

            if (!IsValidSide(arena, player.Id, y))
                throw new InvalidArenaSideException();

            ArenaEntity entity = _attackEntityFactory.CreateEntity(player, card, x, y);
            arena.PlaceEntity(entity);

            return entity;
        }

        public List<ArenaEntity> GetEntities(Arena arena)
        {
            return arena.GetAttackEntities().Where(e => e.IsAlive()).ToList();
        }

        public List<Tower> GetTowers(Arena arena)
        {
            return arena.GetAllTowers().Where(t => t.IsAlive()).ToList();
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

        public IEnumerable<ArenaEntity> GetEnemiesInVision(Arena arena, Positioned positioned)
        {
            double vision = positioned switch
            {
                TroopEntity troop => (troop.PlayerCard.Card as TroopCard)!.VisionRange,
                Tower tower => tower.TowerTemplate.Range,
                _ => 0,
            };

            foreach (
                ArenaEntity enemy in arena
                    .Entities.Where(kvp => kvp.Key != positioned.UserId)
                    .SelectMany(kvp =>
                        from enemy in kvp.Value
                        let distance = CalculateDistance(positioned, enemy)
                        where distance <= vision
                        select enemy
                    )
            )
            {
                yield return enemy;
            }
        }

        public Tower GetNearestEnemyTower(Arena arena, TroopEntity troop)
        {
            IEnumerable<Tower> enemyTowers = arena
                .Towers.Where(kvp => kvp.Key != troop.UserId)
                .SelectMany(kvp => kvp.Value);

            return enemyTowers.OrderBy(t => CalculateDistance(troop, t)).FirstOrDefault()
                ?? throw new EnemyTowersNotFoundException();
        }

        public bool CanExecuteMovement(Arena arena, ArenaEntity troop, int x, int y)
        {
            return arena.IsInsideBounds(x, y) && arena.Grid[y][x].IsWalkable(troop);
        }

        public void KillPositioned(Arena arena, Positioned positioned)
        {
            switch (positioned)
            {
                case ArenaEntity arenaEntity:
                    arena.KillArenaEntity(arenaEntity);
                    break;
                case Tower tower:
                    arena.KillTower(tower);
                    break;
            }
        }

        public (int towersWinner, int towersLosser) GetNumberTowers(
            Arena arena,
            Guid winnerId,
            Guid losserId
        )
        {
            int towersWinner = arena.Towers[winnerId].Count(t => t.IsAlive());
            int towersLosser = arena.Towers[losserId].Count(t => t.IsAlive());

            return (towersWinner, towersLosser);
        }

        private static bool IsValidSide(Arena arena, Guid playerId, int y)
        {
            List<Guid> playerIds = arena.Towers.Keys.ToList();
            Guid player1 = playerIds[0];
            Guid player2 = playerIds[1];

            if (playerId == player1 && y > 13)
                return false;

            if (playerId == player2 && y < 16)
                return false;

            return true;
        }
    }
}
