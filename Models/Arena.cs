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

        public Arena(ArenaTemplate arenaTemplate, Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate = arenaTemplate;
            Towers = towers;

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

            PlaceTower(7, 10, 0, 3, p1Leader);
            PlaceTower(2, 4, 4, 6, p1Guardians[0]);
            PlaceTower(13, 15, 4, 6, p1Guardians[1]);

            PlaceTower(7, 10, 26, 29, p2Leader);
            PlaceTower(2, 4, 23, 25, p2Guardians[0]);
            PlaceTower(13, 15, 23, 25, p2Guardians[1]);
        }

        private void PlaceTower(int colStart, int colEnd, int rowStart, int rowEnd, Tower tower)
        {
            for (int r = rowStart; r <= rowEnd; r++)
            {
                for (int c = colStart; c <= colEnd; c++)
                {
                    if (r >= 0 && r < Height && c >= 0 && c < Width)
                    {
                        Grid[r][c].Tower = tower;
                    }
                }
            }
        }
    }
}