using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class ArenaTests
{
    private Arena CreateTestArena()
    {
        var arenaTemplate = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };

        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var leaderTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3
        };

        var player1Towers = new List<Tower>
        {
            new Tower(leaderTemplate, player1Id),
            new Tower(guardianTemplate, player1Id),
            new Tower(guardianTemplate, player1Id)
        };

        var player2Towers = new List<Tower>
        {
            new Tower(leaderTemplate, player2Id),
            new Tower(guardianTemplate, player2Id),
            new Tower(guardianTemplate, player2Id)
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, player1Towers },
            { player2Id, player2Towers }
        };

        return new Arena(arenaTemplate, towers);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeGridWithCorrectDimensions()
    {
        var arena = CreateTestArena();

        arena.Grid.Should().HaveCount(30);
        arena.Grid[0].Should().HaveCount(18);
    }

    [Fact]
    public void Constructor_ShouldPlaceGroundCells()
    {
        var arena = CreateTestArena();

        var groundCellsCount = 0;
        for (int r = 0; r < 30; r++)
        {
            for (int c = 0; c < 18; c++)
            {
                if (arena.Grid[r][c].Type == CellType.Ground)
                {
                    groundCellsCount++;
                }
            }
        }

        groundCellsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_ShouldPlaceRiverAtCorrectPosition()
    {
        var arena = CreateTestArena();

        for (int c = 0; c < 18; c++)
        {
            arena.Grid[14][c].Type.Should().BeOneOf(CellType.River, CellType.Bridge);
            arena.Grid[15][c].Type.Should().BeOneOf(CellType.River, CellType.Bridge);
        }
    }

    [Fact]
    public void Constructor_ShouldPlaceBridgesAtCorrectPositions()
    {
        var arena = CreateTestArena();

        // Left bridge
        arena.Grid[14][3].Type.Should().Be(CellType.Bridge);
        arena.Grid[15][3].Type.Should().Be(CellType.Bridge);

        // Right bridge
        arena.Grid[14][14].Type.Should().Be(CellType.Bridge);
        arena.Grid[15][14].Type.Should().Be(CellType.Bridge);
    }

    [Fact]
    public void Constructor_ShouldPlaceTowersForBothPlayers()
    {
        var arena = CreateTestArena();

        var towersPlaced = 0;
        for (int r = 0; r < 30; r++)
        {
            for (int c = 0; c < 18; c++)
            {
                if (arena.Grid[r][c].Tower != null)
                {
                    towersPlaced++;
                }
            }
        }

        towersPlaced.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_ShouldPlacePlayer1LeaderTowerInCorrectArea()
    {
        var arena = CreateTestArena();

        var leaderFound = false;
        for (int r = 0; r <= 3; r++)
        {
            for (int c = 7; c <= 10; c++)
            {
                if (arena.Grid[r][c].Tower?.TowerTemplate.Type == TowerType.Leader)
                {
                    leaderFound = true;
                    break;
                }
            }
        }

        leaderFound.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldPlacePlayer2LeaderTowerInCorrectArea()
    {
        var arena = CreateTestArena();

        var leaderFound = false;
        for (int r = 26; r <= 29; r++)
        {
            for (int c = 7; c <= 10; c++)
            {
                if (arena.Grid[r][c].Tower?.TowerTemplate.Type == TowerType.Leader)
                {
                    leaderFound = true;
                    break;
                }
            }
        }

        leaderFound.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldHaveCorrectNumberOfCells()
    {
        var arena = CreateTestArena();

        var totalCells = 0;
        for (int r = 0; r < 30; r++)
        {
            for (int c = 0; c < 18; c++)
            {
                if (arena.Grid[r][c] != null)
                {
                    totalCells++;
                }
            }
        }

        totalCells.Should().Be(30 * 18);
    }

    #endregion
}
