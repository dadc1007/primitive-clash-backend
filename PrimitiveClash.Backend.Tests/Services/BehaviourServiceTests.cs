using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class BehaviourServiceTests
{
    private readonly Mock<IArenaService> _mockArenaService;
    private readonly Mock<IBattleService> _mockBattleService;
    private readonly Mock<IPathfindingService> _mockPathfindingService;
    private readonly Mock<ILogger<BehaviourService>> _mockLogger;
    private readonly BehaviourService _behaviourService;

    public BehaviourServiceTests()
    {
        _mockArenaService = new Mock<IArenaService>();
        _mockBattleService = new Mock<IBattleService>();
        _mockPathfindingService = new Mock<IPathfindingService>();
        _mockLogger = new Mock<ILogger<BehaviourService>>();

        _behaviourService = new BehaviourService(
            _mockArenaService.Object,
            _mockBattleService.Object,
            _mockPathfindingService.Object,
            _mockLogger.Object
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

    private TroopEntity CreateTestTroop(Guid userId, int x, int y, int range = 1, int visionRange = 5)
    {
        var cardId = Guid.NewGuid();
        var card = new TroopCard
        {
            Id = cardId,
            Name = "Test Troop",
            Rarity = CardRarity.Common,
            ElixirCost = 1,
            Hp = 100,
            Damage = 10,
            Range = range,
            MovementSpeed = MovementSpeed.Medium,
            Targets = [UnitClass.Ground],
            UnitClass = UnitClass.Ground,
            ImageUrl = "test.png",
            VisionRange = visionRange
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Card = card,
            Level = 1
        };

        return new TroopEntity(userId, playerCard, x, y);
    }

    #region ExecuteAction - Dead Unit Tests

    [Fact]
    public void ExecuteAction_WithDeadUnit_ShouldDoNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(Guid.NewGuid(), 5, 5);
        troop.TakeDamage(200); // Kill the troop

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        _mockBattleService.Verify(
            x => x.HandleAttack(It.IsAny<Guid>(), It.IsAny<Arena>(), It.IsAny<Positioned>(), It.IsAny<Positioned>()),
            Times.Never
        );
        _mockBattleService.Verify(
            x => x.HandleMovement(It.IsAny<Guid>(), It.IsAny<TroopEntity>(), It.IsAny<Arena>()),
            Times.Never
        );
    }

    #endregion

    #region ExecuteAction - Troop Tests

    [Fact]
    public void ExecuteAction_TroopInRangeOfEnemy_ShouldAttack()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 2);
        var enemy = CreateTestTroop(enemyId, 6, 5);

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity> { enemy });

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemy))
            .Returns(1);

        _mockBattleService
            .Setup(x => x.HandleAttack(sessionId, arena, troop, enemy))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Attacking);
        _mockBattleService.Verify(
            x => x.HandleAttack(sessionId, arena, troop, enemy),
            Times.Once
        );
    }

    [Fact]
    public void ExecuteAction_TroopOutOfRangeOfEnemy_ShouldMoveTowards()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 1);
        var enemy = CreateTestTroop(enemyId, 10, 10);

        var path = new List<Point> { new Point(6, 5), new Point(7, 6) };

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity> { enemy });

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemy))
            .Returns(10);

        _mockPathfindingService
            .Setup(x => x.FindPath(arena, troop, enemy))
            .Returns(path);

        _mockArenaService
            .Setup(x => x.CanExecuteMovement(arena, troop, It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _mockBattleService
            .Setup(x => x.HandleMovement(sessionId, troop, arena))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Moving);
        troop.Path.Should().HaveCount(2);
        _mockBattleService.Verify(
            x => x.HandleMovement(sessionId, troop, arena),
            Times.Once
        );
    }

    [Fact]
    public void ExecuteAction_TroopWithNoEnemiesInVision_ShouldMoveToTower()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 1);
        var enemyTower = new Tower(enemyId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 10,
            Y = 20
        };

        var path = new List<Point> { new Point(6, 5), new Point(7, 6) };

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity>());

        _mockArenaService
            .Setup(x => x.GetNearestEnemyTower(arena, troop))
            .Returns(enemyTower);

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemyTower))
            .Returns(20);

        _mockPathfindingService
            .Setup(x => x.FindPath(arena, troop, enemyTower))
            .Returns(path);

        _mockArenaService
            .Setup(x => x.CanExecuteMovement(arena, troop, It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _mockBattleService
            .Setup(x => x.HandleMovement(sessionId, troop, arena))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Moving);
        troop.Path.Should().HaveCount(2);
        _mockBattleService.Verify(
            x => x.HandleMovement(sessionId, troop, arena),
            Times.Once
        );
    }

    [Fact]
    public void ExecuteAction_TroopInRangeOfTower_ShouldAttackTower()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 3);
        var enemyTower = new Tower(enemyId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 7,
            Y = 7
        };

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity>());

        _mockArenaService
            .Setup(x => x.GetNearestEnemyTower(arena, troop))
            .Returns(enemyTower);

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemyTower))
            .Returns(2);

        _mockBattleService
            .Setup(x => x.HandleAttack(sessionId, arena, troop, enemyTower))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Attacking);
        _mockBattleService.Verify(
            x => x.HandleAttack(sessionId, arena, troop, enemyTower),
            Times.Once
        );
    }

    #endregion

    #region ExecuteAction - Tower Tests

    [Fact]
    public void ExecuteAction_TowerWithEnemyInRange_ShouldAttack()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        
        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 5
        };

        var enemy = CreateTestTroop(enemyId, 7, 7);

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, tower))
            .Returns(new List<ArenaEntity> { enemy });

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(tower, enemy))
            .Returns(2);

        _mockBattleService
            .Setup(x => x.HandleAttack(sessionId, arena, tower, enemy))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, tower);

        // Assert
        tower.State.Should().Be(PositionedState.Attacking);
        _mockBattleService.Verify(
            x => x.HandleAttack(sessionId, arena, tower, enemy),
            Times.Once
        );
    }

    [Fact]
    public void ExecuteAction_TowerWithNoEnemiesInRange_ShouldDoNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arena = CreateTestArena();
        
        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 5
        };

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, tower))
            .Returns(new List<ArenaEntity>());

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, tower);

        // Assert
        _mockBattleService.Verify(
            x => x.HandleAttack(It.IsAny<Guid>(), It.IsAny<Arena>(), It.IsAny<Positioned>(), It.IsAny<Positioned>()),
            Times.Never
        );
    }

    #endregion

    #region ExecuteAction - Continue Current Attack Tests

    [Fact]
    public void ExecuteAction_WithCurrentTargetStillInRange_ShouldContinueAttacking()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 2);
        var enemy = CreateTestTroop(enemyId, 6, 5);

        troop.CurrentTargetId = enemy.Id;
        troop.CurrentTargetIsTower = false;

        _mockArenaService
            .Setup(x => x.GetEntities(arena))
            .Returns(new List<ArenaEntity> { enemy });

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemy))
            .Returns(1);

        _mockBattleService
            .Setup(x => x.HandleAttack(sessionId, arena, troop, enemy))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Attacking);
        _mockBattleService.Verify(
            x => x.HandleAttack(sessionId, arena, troop, enemy),
            Times.Once
        );
    }

    [Fact]
    public void ExecuteAction_WithCurrentTargetDead_ShouldResetAndFindNewTarget()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 2);
        var deadEnemy = CreateTestTroop(enemyId, 6, 5);
        deadEnemy.TakeDamage(200); // Kill enemy

        troop.CurrentTargetId = deadEnemy.Id;
        troop.CurrentTargetIsTower = false;

        _mockArenaService
            .Setup(x => x.GetEntities(arena))
            .Returns(new List<ArenaEntity> { deadEnemy });

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity>());

        _mockArenaService
            .Setup(x => x.GetNearestEnemyTower(arena, troop))
            .Returns((Tower)null!);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.State.Should().Be(PositionedState.Idle);
        troop.CurrentTargetId.Should().BeNull();
    }

    [Fact]
    public void ExecuteAction_TroopWithBlockedPath_ShouldRecalculatePath()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enemyId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5, range: 1);
        var enemy = CreateTestTroop(enemyId, 10, 10);

        var initialPath = new List<Point> { new Point(6, 5), new Point(7, 6) };
        var newPath = new List<Point> { new Point(5, 6), new Point(6, 7) };

        _mockArenaService
            .Setup(x => x.GetEnemiesInVision(arena, troop))
            .Returns(new List<ArenaEntity> { enemy });

        _mockArenaService
            .Setup(x => x.CalculateChebyshevDistance(troop, enemy))
            .Returns(10);

        _mockPathfindingService
            .SetupSequence(x => x.FindPath(arena, troop, enemy))
            .Returns(initialPath)
            .Returns(newPath);

        _mockArenaService
            .Setup(x => x.CanExecuteMovement(arena, troop, 6, 5))
            .Returns(false);

        _mockBattleService
            .Setup(x => x.HandleMovement(sessionId, troop, arena))
            .Returns(Task.CompletedTask);

        // Act
        _behaviourService.ExecuteAction(sessionId, arena, troop);

        // Assert
        troop.Path.Should().HaveCount(2);
        var firstPoint = troop.Path.First();
        firstPoint.X.Should().Be(5);
        firstPoint.Y.Should().Be(6);
        _mockPathfindingService.Verify(
            x => x.FindPath(arena, troop, enemy),
            Times.Exactly(2)
        );
    }

    #endregion
}
