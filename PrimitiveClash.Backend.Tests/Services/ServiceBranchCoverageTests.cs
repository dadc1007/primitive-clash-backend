using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

public class ServiceBranchCoverageTests
{
    #region GameLoopService - Session Removed During Processing

    [Fact]
    public async Task GameLoopService_ProcessTick_SessionRemovedDuringProcessing_SkipsProcessing()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockLogger = new Mock<ILogger<GameLoopService>>();
        var mockGameService = new Mock<IGameService>();
        var mockBehaviorService = new Mock<IBehaviorService>();
        var mockArenaService = new Mock<IArenaService>();

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IGameService)))
            .Returns(mockGameService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IBehaviorService)))
            .Returns(mockBehaviorService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IArenaService)))
            .Returns(mockArenaService.Object);

        var service = new GameLoopService(mockScopeFactory.Object, mockLogger.Object);
        var sessionId = Guid.NewGuid();

        // Start the session
        service.StartGameLoop(sessionId);

        // Immediately stop it to simulate race condition
        service.StopGameLoop(sessionId);

        // Act
        await service.ProcessTick();

        // Assert - GetGame should never be called because session was removed
        mockGameService.Verify(x => x.GetGame(It.IsAny<Guid>()), Times.Never, 
            "Should not process game when session is removed before processing");
    }

    #endregion

    #region NotificationService - Exception in RefreshHand

    [Fact]
    public async Task NotificationService_NotifyCardSpawned_RefreshHandThrowsException_LogsError()
    {
        // Arrange
        var mockHubContext = new Mock<IHubContext<GameHub>>();
        var mockLogger = new Mock<ILogger<NotificationService>>();
        var mockClients = new Mock<IHubClients>();
        var mockGroupProxy = new Mock<IClientProxy>();
        var mockSingleClientProxy = new Mock<ISingleClientProxy>();

        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroupProxy.Object);
        mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(mockSingleClientProxy.Object);

        // Setup SendAsync to throw exception when called with RefreshHand
        mockSingleClientProxy
            .Setup(x => x.SendCoreAsync(
                "RefreshHand",
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new InvalidOperationException("SignalR connection lost"));

        var service = new NotificationService(mockHubContext.Object, mockLogger.Object);

        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var troopCard = new TroopCard
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

        var cardId = troopCard.Id;
        var playerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = troopCard, 
            CardId = cardId,
            UserId = userId,
            Level = 1 
        };
        
        var cardToPut = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = troopCard, 
            CardId = cardId,
            UserId = userId,
            Level = 1 
        };

        var entity = new TroopEntity(playerId, playerCard, 5, 10)
        {
            Id = Guid.NewGuid(),
            Health = 1000
        };

        var playerState = new PlayerState(playerId, "Player1", new List<PlayerCard> 
        { 
            playerCard, cardToPut, playerCard, playerCard, playerCard 
        })
        {
            ConnectionId = "valid-connection-id",
            IsConnected = true
        };

        // Act
        await service.NotifyCardSpawned(sessionId, playerState, entity, cardToPut);

        // Assert - Should log error when exception occurs
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error enviando RefreshHand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log error when RefreshHand throws exception"
        );

        // CardSpawned should still be sent to group
        mockGroupProxy.Verify(
            x => x.SendCoreAsync(
                "CardSpawned",
                It.IsAny<object[]>(),
                default),
            Times.Once,
            "CardSpawned notification should still be sent despite RefreshHand error"
        );
    }

    #endregion

    #region ArenaService - GetEnemiesInVision with BuildingEntity

    [Fact]
    public void ArenaService_GetEnemiesInVision_WithBuildingEntity_UsesZeroVision()
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

        var buildingCard = new BuildingCard
        {
            Id = Guid.NewGuid(),
            Name = "Cannon",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Building,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 500,
            Damage = 150,
            HitSpeed = 0.8f,
            Range = 6,
            DamageArea = 0,
            UnitClass = UnitClass.Buildings,
            Duration = 40
        };

        var cardId = buildingCard.Id;
        var playerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = buildingCard, 
            CardId = cardId,
            UserId = player1Id,
            Level = 1 
        };

        var buildingEntity = new BuildingEntity(player1Id, playerCard, 5, 10)
        {
            Id = Guid.NewGuid(),
            Health = 500
        };

        arena.PlaceEntity(buildingEntity);

        // Create enemy troop nearby (within typical vision range but building has 0 vision)
        var enemyCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Enemy",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 300,
            Damage = 50,
            HitSpeed = 1.0f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Fast,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var enemyCardId = enemyCard.Id;
        var enemyPlayerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = enemyCard, 
            CardId = enemyCardId,
            UserId = player2Id,
            Level = 1 
        };

        var enemyEntity = new TroopEntity(player2Id, enemyPlayerCard, 6, 11)
        {
            Id = Guid.NewGuid(),
            Health = 300
        };

        arena.PlaceEntity(enemyEntity);

        // Act
        var enemiesInVision = service.GetEnemiesInVision(arena, buildingEntity).ToList();

        // Assert
        enemiesInVision.Should().BeEmpty("BuildingEntity should have 0 vision range");
    }

    #endregion

    #region BattleService - Attack Dead Entity

    [Fact]
    public async Task BattleService_HandleAttack_TargetAlreadyDead_ReturnsEarly()
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

        var attackCard = new TroopCard
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

        var attackCardId = attackCard.Id;
        var attackPlayerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = attackCard, 
            CardId = attackCardId,
            UserId = player1Id,
            Level = 1 
        };

        var attacker = new TroopEntity(player1Id, attackPlayerCard, 5, 10)
        {
            Id = Guid.NewGuid(),
            Health = 1000
        };

        var targetCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Enemy",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 300,
            Damage = 50,
            HitSpeed = 1.0f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Fast,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var targetCardId = targetCard.Id;
        var targetPlayerCard = new PlayerCard 
        { 
            Id = Guid.NewGuid(), 
            Card = targetCard, 
            CardId = targetCardId,
            UserId = player2Id,
            Level = 1 
        };

        var target = new TroopEntity(player2Id, targetPlayerCard, 6, 11)
        {
            Id = Guid.NewGuid(),
            Health = 0 // Already dead
        };

        // Act
        await service.HandleAttack(sessionId, arena, attacker, target);

        // Assert - No notifications should be sent for dead target
        mockNotificationService.Verify(
            x => x.NotifyUnitDamaged(It.IsAny<Guid>(), It.IsAny<UnitDamagedNotification>()),
            Times.Never,
            "Should not notify damage when target is already dead"
        );
        
        mockNotificationService.Verify(
            x => x.NotifyUnitKilled(It.IsAny<Guid>(), It.IsAny<UnitKilledNotificacion>()),
            Times.Never,
            "Should not notify kill when target is already dead"
        );
    }

    #endregion
}
