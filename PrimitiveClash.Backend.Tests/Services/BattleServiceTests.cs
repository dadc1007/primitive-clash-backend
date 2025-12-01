using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class BattleServiceTests
{
    private readonly Mock<IGameService> _mockGameService;
    private readonly Mock<IArenaService> _mockArenaService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<BattleService>> _mockLogger;
    private readonly BattleService _battleService;

    public BattleServiceTests()
    {
        _mockGameService = new Mock<IGameService>();
        _mockArenaService = new Mock<IArenaService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<BattleService>>();

        _battleService = new BattleService(
            _mockGameService.Object,
            _mockArenaService.Object,
            _mockNotificationService.Object,
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

    private TroopEntity CreateTestTroop(Guid userId, int x, int y, int damage = 10, int hp = 100)
    {
        var cardId = Guid.NewGuid();
        var card = new TroopCard
        {
            Id = cardId,
            Name = "Test Troop",
            Rarity = CardRarity.Common,
            ElixirCost = 1,
            Hp = hp,
            Damage = damage,
            Range = 1,
            MovementSpeed = MovementSpeed.Medium,
            Targets = [UnitClass.Ground],
            UnitClass = UnitClass.Ground,
            ImageUrl = "test.png"
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

    private PlayerCard CreatePlayerCard(Guid userId, int elixirCost = 3)
    {
        var cardId = Guid.NewGuid();
        var card = new TroopCard
        {
            Id = cardId,
            Name = "Test Card",
            Rarity = CardRarity.Common,
            ElixirCost = elixirCost,
            Hp = 100,
            Damage = 10,
            Range = 1,
            MovementSpeed = MovementSpeed.Medium,
            Targets = [UnitClass.Ground],
            UnitClass = UnitClass.Ground,
            ImageUrl = "test.png"
        };

        return new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Card = card,
            Level = 1
        };
    }

    #region HandleAttack Tests

    [Fact]
    public async Task HandleAttack_TroopAttacksTower_ShouldDealDamage()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();

        var attacker = CreateTestTroop(attackerId, 5, 5, damage: 50);
        var target = new Tower(defenderId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        });

        int initialHealth = target.Health;

        _mockNotificationService
            .Setup(x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await _battleService.HandleAttack(sessionId, arena, attacker, target);

        // Assert
        target.Health.Should().Be(initialHealth - 50);
        _mockNotificationService.Verify(
            x => x.NotifyUnitDamaged(sessionId, It.IsAny<UnitDamagedNotification>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAttack_TowerAttacksTroop_ShouldDealDamage()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();

        var attacker = new Tower(attackerId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 150,
            Range = 5,
            Size = 1
        });

        var target = CreateTestTroop(defenderId, 5, 5, hp: 200);
        int initialHealth = target.Health;

        _mockNotificationService
            .Setup(x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await _battleService.HandleAttack(sessionId, arena, attacker, target);

        // Assert
        target.Health.Should().Be(initialHealth - 150);
        _mockNotificationService.Verify(
            x => x.NotifyUnitDamaged(sessionId, It.IsAny<UnitDamagedNotification>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAttack_KillsTarget_ShouldNotifyAndUpdateState()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();

        var attacker = CreateTestTroop(attackerId, 5, 5, damage: 200);
        var target = CreateTestTroop(defenderId, 6, 6, hp: 100);

        _mockNotificationService
            .Setup(x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService
            .Setup(x => x.NotifyUnitKilled(It.IsAny<Guid>(), It.IsAny<UnitKilledNotificacion>()))
            .Returns(Task.CompletedTask);

        _mockArenaService
            .Setup(x => x.KillPositioned(It.IsAny<Arena>(), It.IsAny<Positioned>()));

        // Act
        await _battleService.HandleAttack(sessionId, arena, attacker, target);

        // Assert
        target.IsAlive().Should().BeFalse();
        attacker.State.Should().Be(PositionedState.Idle);
        _mockNotificationService.Verify(
            x => x.NotifyUnitKilled(sessionId, It.IsAny<UnitKilledNotificacion>()),
            Times.Once
        );
        _mockArenaService.Verify(
            x => x.KillPositioned(arena, target),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAttack_KillsTower_ShouldEndGame()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();

        var attacker = CreateTestTroop(attackerId, 5, 5, damage: 2000);
        var target = new Tower(defenderId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        });

        _mockNotificationService
            .Setup(x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService
            .Setup(x => x.NotifyUnitKilled(It.IsAny<Guid>(), It.IsAny<UnitKilledNotificacion>()))
            .Returns(Task.CompletedTask);

        _mockArenaService
            .Setup(x => x.KillPositioned(It.IsAny<Arena>(), It.IsAny<Positioned>()));

        _mockGameService
            .Setup(x => x.EndGame(sessionId, arena, attackerId, defenderId))
            .Returns(Task.CompletedTask);

        // Act
        await _battleService.HandleAttack(sessionId, arena, attacker, target);

        // Assert
        target.IsAlive().Should().BeFalse();
        _mockGameService.Verify(
            x => x.EndGame(sessionId, arena, attackerId, defenderId),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAttack_TargetAlreadyDead_ShouldDoNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var arena = CreateTestArena();
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();

        var attacker = CreateTestTroop(attackerId, 5, 5, damage: 100);
        var target = CreateTestTroop(defenderId, 6, 6, hp: 100);

        // Kill the target first
        target.TakeDamage(200);

        // Act
        await _battleService.HandleAttack(sessionId, arena, attacker, target);

        // Assert
        _mockNotificationService.Verify(
            x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()),
            Times.Never
        );
    }

    #endregion

    #region HandleMovement Tests

    [Fact]
    public async Task HandleMovement_WithValidPath_ShouldMoveTroop()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5);

        troop.Path.Enqueue(new Point(6, 5));
        troop.Path.Enqueue(new Point(7, 5));

        arena.PlaceEntity(troop);

        _mockNotificationService
            .Setup(x => x.NotifyTroopMoved(It.IsAny<Guid>(), It.IsAny<TroopMovedNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await _battleService.HandleMovement(sessionId, troop, arena);

        // Assert
        troop.X.Should().Be(6);
        troop.Y.Should().Be(5);
        troop.Path.Count.Should().Be(1);
        troop.State.Should().Be(PositionedState.Moving);
        _mockNotificationService.Verify(
            x => x.NotifyTroopMoved(sessionId, It.IsAny<TroopMovedNotification>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleMovement_WithEmptyPath_ShouldDoNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5);

        int initialX = troop.X;
        int initialY = troop.Y;

        // Act
        await _battleService.HandleMovement(sessionId, troop, arena);

        // Assert
        troop.X.Should().Be(initialX);
        troop.Y.Should().Be(initialY);
        _mockNotificationService.Verify(
            x => x.NotifyTroopMoved(It.IsAny<Guid>(), It.IsAny<TroopMovedNotification>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleMovement_MultipleSteps_ShouldMoveSequentially()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var arena = CreateTestArena();
        var troop = CreateTestTroop(userId, 5, 5);

        troop.Path.Enqueue(new Point(6, 5));
        troop.Path.Enqueue(new Point(7, 5));
        troop.Path.Enqueue(new Point(8, 5));

        arena.PlaceEntity(troop);

        _mockNotificationService
            .Setup(x => x.NotifyTroopMoved(It.IsAny<Guid>(), It.IsAny<TroopMovedNotification>()))
            .Returns(Task.CompletedTask);

        // Act - First move
        await _battleService.HandleMovement(sessionId, troop, arena);

        // Assert - After first move
        troop.X.Should().Be(6);
        troop.Y.Should().Be(5);
        troop.Path.Count.Should().Be(2);

        // Act - Second move
        await _battleService.HandleMovement(sessionId, troop, arena);

        // Assert - After second move
        troop.X.Should().Be(7);
        troop.Y.Should().Be(5);
        troop.Path.Count.Should().Be(1);

        _mockNotificationService.Verify(
            x => x.NotifyTroopMoved(sessionId, It.IsAny<TroopMovedNotification>()),
            Times.Exactly(2)
        );
    }

    #endregion

    #region SpawnCard Tests

    [Fact]
    public async Task SpawnCard_WithSufficientElixir_ShouldSpawnCard()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var playerCard = CreatePlayerCard(userId, elixirCost: 3);
        playerCard.Id = cardId;

        // Player needs at least 5 cards (4 in hand + 1 next)
        var cards = new List<PlayerCard>
        {
            playerCard,
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId)
        };

        var game = new Game(
            sessionId,
            new List<PlayerState>(),
            CreateTestArena()
        )
        {
            State = GameState.InProgress
        };

        var playerState = new PlayerState(userId, cards);
        playerState.CurrentElixir = 5;

        var entity = CreateTestTroop(userId, 5, 5);

        _mockGameService
            .Setup(x => x.GetGame(sessionId))
            .ReturnsAsync(game);

        _mockGameService
            .Setup(x => x.GetPlayerState(game, userId))
            .Returns(playerState);

        _mockArenaService
            .Setup(x => x.CreateEntity(It.IsAny<Arena>(), playerState, playerCard, 5, 5))
            .Returns(entity);

        _mockGameService
            .Setup(x => x.SaveGame(game))
            .Returns(Task.CompletedTask);

        _mockNotificationService
            .Setup(x => x.NotifyCardSpawned(It.IsAny<Guid>(), It.IsAny<PlayerState>(), It.IsAny<ArenaEntity>(), It.IsAny<PlayerCard>()))
            .Returns(Task.CompletedTask);

        // Act
        await _battleService.SpawnCard(sessionId, userId, cardId, 5, 5);

        // Assert
        playerState.CurrentElixir.Should().Be(2); // 5 - 3
        _mockGameService.Verify(x => x.SaveGame(game), Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyCardSpawned(sessionId, playerState, entity, It.IsAny<PlayerCard>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SpawnCard_WithInsufficientElixir_ShouldThrowException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var playerCard = CreatePlayerCard(userId, elixirCost: 5);
        playerCard.Id = cardId;

        var cards = new List<PlayerCard>
        {
            playerCard,
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId)
        };

        var game = new Game(
            sessionId,
            new List<PlayerState>(),
            CreateTestArena()
        )
        {
            State = GameState.InProgress
        };

        var playerState = new PlayerState(userId, cards);
        playerState.CurrentElixir = 3;

        _mockGameService
            .Setup(x => x.GetGame(sessionId))
            .ReturnsAsync(game);

        _mockGameService
            .Setup(x => x.GetPlayerState(game, userId))
            .Returns(playerState);

        // Act & Assert
        await Assert.ThrowsAsync<NotEnoughElixirException>(
            () => _battleService.SpawnCard(sessionId, userId, cardId, 5, 5)
        );

        playerState.CurrentElixir.Should().Be(3); // Unchanged
        _mockGameService.Verify(x => x.SaveGame(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task SpawnCard_WithInvalidCard_ShouldThrowException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var invalidCardId = Guid.NewGuid();

        var playerCard = CreatePlayerCard(userId);
        playerCard.Id = cardId;

        var cards = new List<PlayerCard>
        {
            playerCard,
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId),
            CreatePlayerCard(userId)
        };

        var game = new Game(
            sessionId,
            new List<PlayerState>(),
            CreateTestArena()
        )
        {
            State = GameState.InProgress
        };

        var playerState = new PlayerState(userId, cards);

        _mockGameService
            .Setup(x => x.GetGame(sessionId))
            .ReturnsAsync(game);

        _mockGameService
            .Setup(x => x.GetPlayerState(game, userId))
            .Returns(playerState);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCardException>(
            () => _battleService.SpawnCard(sessionId, userId, invalidCardId, 5, 5)
        );
    }

    #endregion
}
