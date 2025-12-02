using FluentAssertions;
using Moq;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class ArenaServiceTests
{
    private readonly Mock<IArenaTemplateService> _mockArenaTemplateService;
    private readonly Mock<IArenaEntityFactory> _mockArenaEntityFactory;
    private readonly ArenaService _arenaService;

    public ArenaServiceTests()
    {
        _mockArenaTemplateService = new Mock<IArenaTemplateService>();
        _mockArenaEntityFactory = new Mock<IArenaEntityFactory>();

        _arenaService = new ArenaService(
            _mockArenaTemplateService.Object,
            _mockArenaEntityFactory.Object
        );
    }

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
            Range = 5,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3,
            Size = 3
        };

        var player1Towers = new List<Tower>
        {
            new Tower(player1Id, leaderTemplate),
            new Tower(player1Id, guardianTemplate),
            new Tower(player1Id, guardianTemplate)
        };

        var player2Towers = new List<Tower>
        {
            new Tower(player2Id, leaderTemplate),
            new Tower(player2Id, guardianTemplate),
            new Tower(player2Id, guardianTemplate)
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, player1Towers },
            { player2Id, player2Towers }
        };

        return new Arena(arenaTemplate, towers);
    }

    private TroopCard CreateTestTroopCard()
    {
        return new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Test Troop",
            Type = CardType.Troop,
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Damage = 10,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "",
            Hp = 100,
            Range = 1,
            DamageArea = 0,
            HitSpeed = 1.0f,
            UnitClass = UnitClass.Ground,
            VisionRange = 5,
            MovementSpeed = MovementSpeed.Fast
        };
    }

    #region CreateArena Tests

    [Fact]
    public async Task CreateArena_ShouldReturnArenaWithTemplate()
    {
        // Arrange
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
            Range = 5,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3,
            Size = 3
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, new List<Tower> { new Tower(player1Id, leaderTemplate), new Tower(player1Id, guardianTemplate), new Tower(player1Id, guardianTemplate) } },
            { player2Id, new List<Tower> { new Tower(player2Id, leaderTemplate), new Tower(player2Id, guardianTemplate), new Tower(player2Id, guardianTemplate) } }
        };

        _mockArenaTemplateService
            .Setup(x => x.GetDefaultArenaTemplate())
            .ReturnsAsync(arenaTemplate);

        // Act
        var result = await _arenaService.CreateArena(towers);

        // Assert
        result.Should().NotBeNull();
        result.ArenaTemplate.Should().Be(arenaTemplate);
        result.Towers.Should().HaveCount(2);
    }

    #endregion

    #region CreateEntity Tests

    [Fact]
    public void CreateEntity_WithValidPosition_ShouldCreateAndPlaceEntity()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var playerState = new PlayerState(playerId, "Player1", new List<PlayerCard> { playerCard })
        {
            CurrentElixir = 10
        };

        var troopEntity = new TroopEntity(playerId, playerCard, 8, 10);

        _mockArenaEntityFactory
            .Setup(x => x.CreateEntity(playerState, playerCard, 8, 10))
            .Returns(troopEntity);

        // Act
        var result = _arenaService.CreateEntity(arena, playerState, playerCard, 8, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TroopEntity>();
        arena.GetAttackEntities().Should().Contain(troopEntity);
    }

    [Fact]
    public void CreateEntity_WithInvalidPosition_ShouldThrowException()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var playerState = new PlayerState(playerId, "Player1", new List<PlayerCard> { playerCard });

        // Act & Assert
        Assert.Throws<InvalidSpawnPositionException>(
            () => _arenaService.CreateEntity(arena, playerState, playerCard, 100, 100)
        );
    }

    [Fact]
    public void CreateEntity_WithInvalidSide_ShouldThrowException()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var playerState = new PlayerState(playerId, "Player1", new List<PlayerCard> { playerCard });

        // Player 1 trying to spawn on player 2's side (y > 13)
        // Act & Assert
        Assert.Throws<InvalidArenaSideException>(
            () => _arenaService.CreateEntity(arena, playerState, playerCard, 8, 20)
        );
    }

    #endregion

    #region GetEntities Tests

    [Fact]
    public void GetEntities_ShouldReturnOnlyAliveEntities()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };

        var aliveTroop = new TroopEntity(playerId, playerCard, 8, 10);
        var deadTroop = new TroopEntity(playerId, playerCard, 9, 10);
        deadTroop.TakeDamage(1000);

        arena.PlaceEntity(aliveTroop);
        arena.PlaceEntity(deadTroop);

        // Act
        var result = _arenaService.GetEntities(arena);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(aliveTroop);
        result.Should().NotContain(deadTroop);
    }

    #endregion

    #region GetTowers Tests

    [Fact]
    public void GetTowers_ShouldReturnOnlyAliveTowers()
    {
        // Arrange
        var arena = CreateTestArena();
        var initialTowerCount = arena.GetAllTowers().Count();
        var firstTower = arena.GetAllTowers().First();

        // Kill first tower
        firstTower.TakeDamage(10000);

        // Act
        var result = _arenaService.GetTowers(arena);

        // Assert
        result.Should().HaveCount(initialTowerCount - 1);
        result.Should().NotContain(firstTower);
    }

    #endregion

    #region PlaceEntity and RemoveEntity Tests

    [Fact]
    public void PlaceEntity_ShouldAddEntityToArena()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var troop = new TroopEntity(playerId, playerCard, 8, 10);

        // Act
        _arenaService.PlaceEntity(arena, troop);

        // Assert
        arena.GetAttackEntities().Should().Contain(troop);
    }

    [Fact]
    public void RemoveEntity_ShouldRemoveEntityFromGrid()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var troop = new TroopEntity(playerId, playerCard, 8, 10);

        arena.PlaceEntity(troop);
        var cellBeforeRemove = arena.Grid[troop.Y][troop.X];
        cellBeforeRemove.GroundEntity.Should().BeTrue();

        // Act
        _arenaService.RemoveEntity(arena, troop);

        // Assert - Cell should not have ground entity anymore
        var cellAfterRemove = arena.Grid[troop.Y][troop.X];
        cellAfterRemove.GroundEntity.Should().BeFalse();
    }

    #endregion

    #region Distance Calculation Tests

    [Fact]
    public void CalculateChebyshevDistance_ShouldReturnMaxDifference()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };

        var entity1 = new TroopEntity(playerId, playerCard, 0, 0);
        var entity2 = new TroopEntity(playerId, playerCard, 3, 4);

        // Act
        var distance = _arenaService.CalculateChebyshevDistance(entity1, entity2);

        // Assert
        distance.Should().Be(4); // Max(|3-0|, |4-0|) = Max(3, 4) = 4
    }

    [Fact]
    public void CalculateEuclideanDistance_ShouldReturnStraightLineDistance()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };

        var entity1 = new TroopEntity(playerId, playerCard, 0, 0);
        var entity2 = new TroopEntity(playerId, playerCard, 3, 4);

        // Act
        var distance = _arenaService.CalculateEuclideanDistance(entity1, entity2);

        // Assert
        distance.Should().Be(5); // sqrt(3^2 + 4^2) = sqrt(25) = 5
    }

    #endregion

    #region GetEnemiesInVision Tests

    [Fact]
    public void GetEnemiesInVision_WithEnemiesInRange_ShouldReturnThem()
    {
        // Arrange
        var arena = CreateTestArena();
        var player1Id = arena.Towers.Keys.First();
        var player2Id = arena.Towers.Keys.Last();

        var card = CreateTestTroopCard();
        var playerCard1 = new PlayerCard
        {
            CardId = card.Id,
            UserId = player1Id,
            Card = card
        };
        var playerCard2 = new PlayerCard
        {
            CardId = card.Id,
            UserId = player2Id,
            Card = card
        };

        var troop1 = new TroopEntity(player1Id, playerCard1, 8, 10);
        var enemyTroop = new TroopEntity(player2Id, playerCard2, 8, 12); // Within vision range

        arena.PlaceEntity(troop1);
        arena.PlaceEntity(enemyTroop);

        // Act
        var enemies = _arenaService.GetEnemiesInVision(arena, troop1).ToList();

        // Assert
        enemies.Should().HaveCount(1);
        enemies.Should().Contain(enemyTroop);
    }

    [Fact]
    public void GetEnemiesInVision_WithEnemiesOutOfRange_ShouldReturnEmpty()
    {
        // Arrange
        var arena = CreateTestArena();
        var player1Id = arena.Towers.Keys.First();
        var player2Id = arena.Towers.Keys.Last();

        var card = CreateTestTroopCard();
        var playerCard1 = new PlayerCard
        {
            CardId = card.Id,
            UserId = player1Id,
            Card = card
        };
        var playerCard2 = new PlayerCard
        {
            CardId = card.Id,
            UserId = player2Id,
            Card = card
        };

        var troop1 = new TroopEntity(player1Id, playerCard1, 8, 10);
        var enemyTroop = new TroopEntity(player2Id, playerCard2, 8, 20); // Out of vision range

        arena.PlaceEntity(troop1);
        arena.PlaceEntity(enemyTroop);

        // Act
        var enemies = _arenaService.GetEnemiesInVision(arena, troop1).ToList();

        // Assert
        enemies.Should().BeEmpty();
    }

    #endregion

    #region GetNearestEnemyTower Tests

    [Fact]
    public void GetNearestEnemyTower_ShouldReturnClosestTower()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };

        var troop = new TroopEntity(playerId, playerCard, 8, 10);
        arena.PlaceEntity(troop);

        // Act
        var nearestTower = _arenaService.GetNearestEnemyTower(arena, troop);

        // Assert
        nearestTower.Should().NotBeNull();
        nearestTower.UserId.Should().NotBe(playerId);
    }

    #endregion

    #region CanExecuteMovement Tests

    [Fact]
    public void CanExecuteMovement_WithValidPosition_ShouldReturnTrue()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var troop = new TroopEntity(playerId, playerCard, 8, 10);

        // Act
        var canMove = _arenaService.CanExecuteMovement(arena, troop, 8, 11);

        // Assert
        canMove.Should().BeTrue();
    }

    [Fact]
    public void CanExecuteMovement_WithInvalidPosition_ShouldReturnFalse()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var troop = new TroopEntity(playerId, playerCard, 8, 10);

        // Act
        var canMove = _arenaService.CanExecuteMovement(arena, troop, -1, 10);

        // Assert
        canMove.Should().BeFalse();
    }

    #endregion

    #region KillPositioned Tests

    [Fact]
    public void KillPositioned_WithArenaEntity_ShouldKillEntity()
    {
        // Arrange
        var arena = CreateTestArena();
        var playerId = arena.Towers.Keys.First();
        var card = CreateTestTroopCard();
        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = playerId,
            Card = card
        };
        var troop = new TroopEntity(playerId, playerCard, 8, 10);
        arena.PlaceEntity(troop);

        // Act
        _arenaService.KillPositioned(arena, troop);

        // Assert - Entity should be removed from arena
        arena.GetAttackEntities().Should().NotContain(e => e.Id == troop.Id);
    }

    [Fact]
    public void KillPositioned_WithTower_ShouldKillTower()
    {
        // Arrange
        var arena = CreateTestArena();
        var tower = arena.GetAllTowers().First();
        var towerId = tower.Id;

        // Act
        _arenaService.KillPositioned(arena, tower);

        // Assert - Tower should be removed from arena
        arena.GetAllTowers().Should().NotContain(t => t.Id == towerId);
    }

    #endregion

    #region GetNumberTowers Tests

    [Fact]
    public void GetNumberTowers_ShouldReturnCorrectCount()
    {
        // Arrange
        var arena = CreateTestArena();
        var winnerId = arena.Towers.Keys.First();
        var loserId = arena.Towers.Keys.Last();

        // Kill one tower of the loser by dealing massive damage
        var loserTower = arena.Towers[loserId].First();
        loserTower.TakeDamage(10000);

        // Act
        var (towersWinner, towersLosser) = _arenaService.GetNumberTowers(arena, winnerId, loserId);

        // Assert
        towersWinner.Should().Be(3); // All 3 towers alive
        towersLosser.Should().Be(2); // 2 towers alive (1 was killed)
    }

    #endregion
}
