using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;

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
        public Dictionary<Guid, List<AttackEntity>> Entities { get; set; }

        public Arena(ArenaTemplate arenaTemplate, Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate = arenaTemplate;
            Towers = towers;
            Entities = towers.Keys.ToDictionary(userId => userId, _ => new List<AttackEntity>());

            Grid = new Cell[Height][];
            for (int i = 0; i < Height; i++)
            {
                Grid[i] = new Cell[Width];
            }

            InitializeLayout();
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
            tower.PosX = colStart;
            tower.PosY = rowStart;

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

        public void PlaceEntity(AttackEntity entity)
        {
            int x = entity.PosX;
            int y = entity.PosY;

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

        public void RemoveEntity(AttackEntity entity)
        {
            int x = entity.PosX;
            int y = entity.PosY;

            Cell cell = Grid[y][x];
            cell.RemoveEntity(entity);
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
    }
}