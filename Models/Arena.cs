using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;
using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models
{
    public class Arena
    {
        private const int Height = 30;
        private const int Width = 18;

        public Guid Id { get; set; } = Guid.NewGuid();
        public ArenaTemplate ArenaTemplate { get; set; }
        public Cell[][] Grid { get; set; }
        public Dictionary<Guid, List<Tower>> Towers { get; set; }
        public Dictionary<Guid, List<ArenaEntity>> Entities { get; set; }

        public Arena(ArenaTemplate arenaTemplate, Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate = arenaTemplate;
            Towers = towers;
            Entities = towers.Keys.ToDictionary(userId => userId, _ => new List<ArenaEntity>());

            Grid = new Cell[Height][];
            for (int i = 0; i < Height; i++)
            {
                Grid[i] = new Cell[Width];
            }

            InitializeLayout();
        }

        [JsonConstructor]
        public Arena(Guid id, ArenaTemplate arenaTemplate, Cell[][] grid, Dictionary<Guid, List<Tower>> towers, Dictionary<Guid, List<ArenaEntity>> entities)
        {
            Id = id;
            ArenaTemplate = arenaTemplate;
            Grid = grid;
            Towers = towers;
            Entities = entities;
        }

        private void InitializeLayout()
        {
            PlaceGround();
            PlaceRiver();
            PlaceBridges();
            PlaceTowers();
        }

        private void PlaceGround()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    Grid[r][c] = new Cell()
                    {
                        Type = CellType.Ground
                    };
                }
            }
        }

        private void PlaceRiver()
        {
            for (int c = 0; c < Width; c++)
            {
                Grid[14][c].Type = CellType.River;
                Grid[15][c].Type = CellType.River;
            }
        }

        private void PlaceBridges()
        {
            // Left bridge
            Grid[14][3].Type = CellType.Bridge;
            Grid[15][3].Type = CellType.Bridge;

            // Rigth bridge
            Grid[14][14].Type = CellType.Bridge;
            Grid[15][14].Type = CellType.Bridge;
        }

        private void PlaceTowers()
        {
            List<Guid> playerIds = [.. Towers.Keys];

            var p1Towers = Towers[playerIds[0]];
            var p1Leader = p1Towers.First(t => t.TowerTemplate.Type == TowerType.Leader);
            var p1Guardians = p1Towers.Where(t => t.TowerTemplate.Type == TowerType.Guardian).ToArray();

            var p2Towers = Towers[playerIds[1]];
            var p2Leader = p2Towers.First(t => t.TowerTemplate.Type == TowerType.Leader);
            var p2Guardians = p2Towers.Where(t => t.TowerTemplate.Type == TowerType.Guardian).ToArray();

            PlaceTower(7, 1, p1Leader);
            PlaceTower(2, 4, p1Guardians[0]);
            PlaceTower(13, 4, p1Guardians[1]);

            PlaceTower(7, 25, p2Leader);
            PlaceTower(2, 23, p2Guardians[0]);
            PlaceTower(13, 23, p2Guardians[1]);
        }

        private void PlaceTower(int colStart, int rowStart, Tower tower)
        {
            tower.X = colStart;
            tower.Y = rowStart;

            foreach (var (c, r) in tower.GetOccupiedCells())
            {
                if (IsInsideBounds(c, r))
                {
                    Grid[r][c].Tower = true;
                }
            }
        }

        public bool IsInsideBounds(int x, int y)
        {
            return !(x < 0 || y < 0 || y >= Height || x >= Width);
        }

        public void PlaceEntity(ArenaEntity entity)
        {
            int x = entity.X;
            int y = entity.Y;

            Cell cell = Grid[y][x];
            bool placed = cell.PlaceEntity(entity);

            if (!placed) throw new InvalidSpawnPositionException(x, y);

            if (Entities.TryGetValue(entity.UserId, out var list))
            {
                if (!list.Any(e => e.Id == entity.Id))
                {
                    list.Add(entity);
                }
            }
        }

        public void RemoveEntity(ArenaEntity entity)
        {
            if (IsInsideBounds(entity.X, entity.Y))
            {
                Cell cell = Grid[entity.Y][entity.X];
                cell.RemoveEntity(entity);
            }
        }

        public void RemoveTower(Tower tower)
        {
            foreach (var (X, Y) in tower.GetOccupiedCells())
            {
                if (IsInsideBounds(X, Y))
                {
                    Cell cell = Grid[Y][X];
                    cell.RemoveTower();
                }
            }
        }

        public IEnumerable<ArenaEntity> GetAttackEntities()
        {
            return Entities.Values.SelectMany(list => list);
        }

        public IEnumerable<Tower> GetAllTowers()
        {
            return Towers.Values.SelectMany(list => list);
        }

        public IEnumerable<TroopEntity> GetAllTroops()
        {
            return GetAttackEntities().OfType<TroopEntity>();
        }

        public void KillArenaEntity(ArenaEntity arenaEntity)
        {
            if (Entities.TryGetValue(arenaEntity.UserId, out var list))
            {
                list.RemoveAll(e => e.Id == arenaEntity.Id);
            }

            RemoveEntity(arenaEntity);
        }

        internal void KillTower(Tower tower)
        {
            if (Towers.TryGetValue(tower.UserId, out var list))
            {
                list.RemoveAll(t => t.Id == tower.Id);
            }

            RemoveTower(tower);
        }
    }
}