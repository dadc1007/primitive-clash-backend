using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Entities;
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

        public bool IsInsideBounds(int x, int y)
        {
            return !(x < 0 || y < 0 || y >= Height || x >= Width);
        }

        public void SpawnEntity(PlayerState player, PlayerCard card, int x, int y)
        {
            if (!IsInsideBounds(x, y))
                throw new InvalidSpawnPositionException(x, y);

            var cell = Grid[y][x];

            var entity = new TroopEntity(player.UserId, card, x, y);

            if (!cell.IsWalkable(entity))
                throw new InvalidSpawnPositionException(x, y);

            if (player.CurrentElixir < card.Card.ElixirCost)
                throw new NotEnoughElixirException(card.Card.ElixirCost, player.CurrentElixir);

            bool placed = cell.TryPlaceEntity(entity);
            if (!placed)
                throw new InvalidSpawnPositionException(x, y);

            player.CurrentElixir -= card.Card.ElixirCost;
        }

        public void RemoveTroop(TroopEntity troop)
        {
            if (!IsInsideBounds(troop.PosX, troop.PosY))
                return;

            var cell = Grid[troop.PosY][troop.PosX];
            var movementType = troop.Card.Card is TroopCard card ? card.MovementType : MovementType.Ground;

            if (movementType == MovementType.Ground)
                cell.GroundEntity = null;
            else
                cell.AirEntity = null;
        }

        public void RemoveTower(Tower tower)
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Grid[r][c].Tower == tower)
                        Grid[r][c].Tower = null;
                }
            }
        }
    }
}