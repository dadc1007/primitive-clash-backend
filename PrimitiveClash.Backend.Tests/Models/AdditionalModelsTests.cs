using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Tests.Models;

public class AdditionalModelsTests
{
    #region Game Tests

    [Fact]
    public void Game_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        
        var playerState1 = new PlayerState(player1Id, "Player1", new List<PlayerCard>());
        var playerState2 = new PlayerState(player2Id, "Player2", new List<PlayerCard>());
        var playerStates = new List<PlayerState> { playerState1, playerState2 };

        var arenaTemplate = new ArenaTemplate { Id = Guid.NewGuid(), Name = "Test Arena", RequiredTrophies = 0 };
        var leaderTemplate = new TowerTemplate { Id = Guid.NewGuid(), Type = TowerType.Leader, Hp = 2000, Damage = 100, Range = 7, Size = 4 };
        var guardianTemplate = new TowerTemplate { Id = Guid.NewGuid(), Type = TowerType.Guardian, Hp = 1000, Damage = 50, Range = 5, Size = 3 };
        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, new List<Tower> { new Tower(player1Id, leaderTemplate), new Tower(player1Id, guardianTemplate), new Tower(player1Id, guardianTemplate) } },
            { player2Id, new List<Tower> { new Tower(player2Id, leaderTemplate), new Tower(player2Id, guardianTemplate), new Tower(player2Id, guardianTemplate) } }
        };
        var arena = new Arena(arenaTemplate, towers);

        // Act
        var game = new Game(id, playerStates, arena);

        // Assert
        game.Id.Should().Be(id);
        game.PlayerStates.Should().HaveCount(2);
        game.GameArena.Should().Be(arena);
        game.State.Should().Be(GameState.InProgress);
    }

    [Fact]
    public void Game_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Game.InitialElixir.Should().Be(5m);
        Game.ElixirPerSecond.Should().Be(1m);
        Game.MaxElixir.Should().Be(10m);
    }

    [Fact]
    public void Game_State_CanBeModified()
    {
        // Arrange
        var id = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var playerStates = new List<PlayerState> 
        { 
            new PlayerState(player1Id, "Player1", new List<PlayerCard>()),
            new PlayerState(player2Id, "Player2", new List<PlayerCard>())
        };
        var arenaTemplate = new ArenaTemplate { Id = Guid.NewGuid(), Name = "Test", RequiredTrophies = 0 };
        var leaderTemplate = new TowerTemplate { Id = Guid.NewGuid(), Type = TowerType.Leader, Hp = 2000, Damage = 100, Range = 7, Size = 4 };
        var guardianTemplate = new TowerTemplate { Id = Guid.NewGuid(), Type = TowerType.Guardian, Hp = 1000, Damage = 50, Range = 5, Size = 3 };
        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, new List<Tower> { new Tower(player1Id, leaderTemplate), new Tower(player1Id, guardianTemplate), new Tower(player1Id, guardianTemplate) } },
            { player2Id, new List<Tower> { new Tower(player2Id, leaderTemplate), new Tower(player2Id, guardianTemplate), new Tower(player2Id, guardianTemplate) } }
        };
        var arena = new Arena(arenaTemplate, towers);
        var game = new Game(id, playerStates, arena);

        // Act
        game.State = GameState.Finished;

        // Assert
        game.State.Should().Be(GameState.Finished);
    }

    #endregion

    #region Cell Tests

    [Fact]
    public void Cell_RemoveTower_ShouldSetTowerToFalse()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground, Tower = true };

        // Act
        cell.RemoveTower();

        // Assert
        cell.Tower.Should().BeFalse();
    }

    [Fact]
    public void Cell_RemoveEntity_WithGroundEntity_ShouldUpdateGroundEntityFlag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 100,
            Damage = 50,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground
        };
        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = card.Id,
            Card = card,
            Level = 1
        };
        var entity = new TroopEntity(userId, playerCard, 5, 5)
        {
            Health = 100
        };
        
        var cell = new Cell { Type = CellType.Ground, GroundEntity = true };

        // Act
        cell.RemoveEntity(entity);

        // Assert
        cell.GroundEntity.Should().BeFalse();
    }

    #endregion

    #region BuildingEntity Tests

    [Fact]
    public void BuildingEntity_Constructor_ShouldSetHealthFromBuildingCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var buildingCard = new BuildingCard
        {
            Id = Guid.NewGuid(),
            Name = "Cannon",
            Targets = [UnitClass.Ground],
            Hp = 500,
            Damage = 100,
            Range = 5,
            HitSpeed = 0.8f,
            Duration = 30
        };
        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = buildingCard.Id,
            Card = buildingCard,
            Level = 1
        };

        // Act
        var buildingEntity = new BuildingEntity(userId, playerCard, 10, 10);

        // Assert
        buildingEntity.Health.Should().Be(500);
        buildingEntity.UserId.Should().Be(userId);
        buildingEntity.PlayerCard.Should().Be(playerCard);
        buildingEntity.X.Should().Be(10);
        buildingEntity.Y.Should().Be(10);
    }

    #endregion

    #region PathfindingNode Tests

    [Fact]
    public void PathfindingNode_Equals_WithNonPathfindingNodeObject_ShouldReturnFalse()
    {
        // Arrange
        var node = new PathfindingNode(5, 10);
        var otherObject = new { X = 5, Y = 10 };

        // Act
        var result = node.Equals(otherObject);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
