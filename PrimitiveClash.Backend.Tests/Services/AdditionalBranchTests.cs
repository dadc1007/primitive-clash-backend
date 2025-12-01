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
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

public class AdditionalBranchTests
{
    #region ArenaService - IsValidSide branches

    [Fact]
    public void ArenaService_CreateEntity_Player1InvalidSide_ThrowsException()
    {
        // Arrange
        var mockArenaTemplateService = new Mock<IArenaTemplateService>();
        var mockArenaEntityFactory = new Mock<IArenaEntityFactory>();
        var service = new ArenaService(mockArenaTemplateService.Object, mockArenaEntityFactory.Object);

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
            { player1Id, new List<Tower> 
                { 
                    new Tower(player1Id, leaderTemplate), 
                    new Tower(player1Id, guardianTemplate), 
                    new Tower(player1Id, guardianTemplate) 
                } 
            },
            { player2Id, new List<Tower> 
                { 
                    new Tower(player2Id, leaderTemplate), 
                    new Tower(player2Id, guardianTemplate), 
                    new Tower(player2Id, guardianTemplate) 
                } 
            }
        };

        var arena = new Arena(arenaTemplate, towers);

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 1000,
            Damage = 100,
            HitSpeed = 1.5f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var cardId = card.Id;
        var playerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = card, 
            CardId = cardId,
            UserId = player1Id,
            Level = 1 
        };

        var playerCards = new List<PlayerCard> { playerCard, playerCard, playerCard, playerCard, playerCard };
        var playerState = new PlayerState(player1Id, playerCards);

        // Act & Assert - Player 1 trying to spawn at y > 13 (invalid)
        var act = () => service.CreateEntity(arena, playerState, playerCard, 8, 14);
        act.Should().Throw<InvalidArenaSideException>("Player 1 cannot spawn at y > 13");
    }

    [Fact]
    public void ArenaService_CreateEntity_Player2InvalidSide_ThrowsException()
    {
        // Arrange
        var mockArenaTemplateService = new Mock<IArenaTemplateService>();
        var mockArenaEntityFactory = new Mock<IArenaEntityFactory>();
        var service = new ArenaService(mockArenaTemplateService.Object, mockArenaEntityFactory.Object);

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
            { player1Id, new List<Tower> 
                { 
                    new Tower(player1Id, leaderTemplate), 
                    new Tower(player1Id, guardianTemplate), 
                    new Tower(player1Id, guardianTemplate) 
                } 
            },
            { player2Id, new List<Tower> 
                { 
                    new Tower(player2Id, leaderTemplate), 
                    new Tower(player2Id, guardianTemplate), 
                    new Tower(player2Id, guardianTemplate) 
                } 
            }
        };

        var arena = new Arena(arenaTemplate, towers);

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 1000,
            Damage = 100,
            HitSpeed = 1.5f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var cardId = card.Id;
        var playerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = card, 
            CardId = cardId,
            UserId = player2Id,
            Level = 1 
        };

        var playerCards = new List<PlayerCard> { playerCard, playerCard, playerCard, playerCard, playerCard };
        var playerState = new PlayerState(player2Id, playerCards);

        // Act & Assert - Player 2 trying to spawn at y < 16 (invalid)
        var act = () => service.CreateEntity(arena, playerState, playerCard, 8, 15);
        act.Should().Throw<InvalidArenaSideException>("Player 2 cannot spawn at y < 16");
    }

    #endregion

    #region BattleService - Tower Attack

    [Fact]
    public async Task BattleService_HandleAttack_TowerKillsEntity_EndsGame()
    {
        // Arrange
        var mockGameService = new Mock<IGameService>();
        var mockArenaService = new Mock<IArenaService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<BattleService>>();

        var service = new BattleService(
            mockGameService.Object,
            mockArenaService.Object,
            mockNotificationService.Object,
            mockLogger.Object
        );

        var sessionId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var arenaTemplate = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };

        var leaderTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 3000,
            Damage = 500,
            Range = 7,
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
            { player1Id, new List<Tower> 
                { 
                    new Tower(player1Id, leaderTemplate), 
                    new Tower(player1Id, guardianTemplate), 
                    new Tower(player1Id, guardianTemplate) 
                } 
            },
            { player2Id, new List<Tower> 
                { 
                    new Tower(player2Id, leaderTemplate) 
                    { 
                        Health = 10 // Low health, will be killed
                    }, 
                    new Tower(player2Id, guardianTemplate), 
                    new Tower(player2Id, guardianTemplate) 
                } 
            }
        };

        var arena = new Arena(arenaTemplate, towers);

        // Create attacker tower - Player 1's leader tower
        var attackerTower = towers[player1Id][0];
        
        // Target is Player 2's leader tower (low health)
        var targetTower = towers[player2Id][0];

        // Setup EndGame expectation
        mockGameService.Setup(x => x.EndGame(
            sessionId,
            arena,
            player1Id,
            player2Id
        )).Returns(Task.CompletedTask);

        // Act
        await service.HandleAttack(sessionId, arena, attackerTower, targetTower);

        // Assert
        targetTower.IsAlive().Should().BeFalse("Tower should be killed");
        
        mockGameService.Verify(
            x => x.EndGame(sessionId, arena, player1Id, player2Id),
            Times.Once,
            "Should call EndGame when tower is killed"
        );

        mockNotificationService.Verify(
            x => x.NotifyUnitKilled(sessionId, It.IsAny<UnitKilledNotificacion>()),
            Times.Once,
            "Should notify unit killed"
        );

        mockArenaService.Verify(
            x => x.KillPositioned(arena, targetTower),
            Times.Once,
            "Should kill the tower"
        );
    }

    #endregion

    #region BattleService - Empty Path Movement

    [Fact]
    public async Task BattleService_HandleMovement_EmptyPath_ReturnsEarly()
    {
        // Arrange
        var mockGameService = new Mock<IGameService>();
        var mockArenaService = new Mock<IArenaService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<BattleService>>();

        var service = new BattleService(
            mockGameService.Object,
            mockArenaService.Object,
            mockNotificationService.Object,
            mockLogger.Object
        );

        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var arenaTemplate = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };

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
            { playerId, new List<Tower> 
                { 
                    new Tower(playerId, leaderTemplate), 
                    new Tower(playerId, guardianTemplate), 
                    new Tower(playerId, guardianTemplate) 
                } 
            },
            { Guid.NewGuid(), new List<Tower> 
                { 
                    new Tower(Guid.NewGuid(), leaderTemplate), 
                    new Tower(Guid.NewGuid(), guardianTemplate), 
                    new Tower(Guid.NewGuid(), guardianTemplate) 
                } 
            }
        };

        var arena = new Arena(arenaTemplate, towers);

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 1000,
            Damage = 100,
            HitSpeed = 1.5f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var cardId = card.Id;
        var playerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = card, 
            CardId = cardId,
            UserId = playerId,
            Level = 1 
        };

        var troop = new TroopEntity(playerId, playerCard, 5, 10)
        {
            Id = Guid.NewGuid(),
            Health = 1000
        };

        // Ensure path is empty
        troop.Path.Clear();

        arena.PlaceEntity(troop);

        // Act
        await service.HandleMovement(sessionId, troop, arena);

        // Assert - Should not notify movement when path is empty
        mockNotificationService.Verify(
            x => x.NotifyTroopMoved(It.IsAny<Guid>(), It.IsAny<TroopMovedNotification>()),
            Times.Never,
            "Should not notify troop movement when path is empty"
        );
    }

    #endregion
}
