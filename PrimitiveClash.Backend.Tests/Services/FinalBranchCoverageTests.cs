using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

/// <summary>
/// Final tests to reach 80% branch coverage target
/// </summary>
public class FinalBranchCoverageTests
{
    #region NotificationService - RefreshHand Exception

    [Fact]
    public async Task NotificationService_NotifyCardSpawned_RefreshHandThrowsException_StillSendsCardSpawned()
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

        // Simulate exception during RefreshHand
        mockSingleClientProxy
            .Setup(x => x.SendCoreAsync("RefreshHand", It.IsAny<object[]>(), default))
            .ThrowsAsync(new InvalidOperationException("Connection lost"));

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

        var playerState = new PlayerState(playerId, new List<PlayerCard> 
        { 
            playerCard, cardToPut, playerCard, playerCard, playerCard 
        })
        {
            ConnectionId = "test-connection-id",
            IsConnected = true
        };

        // Act
        await service.NotifyCardSpawned(sessionId, playerState, entity, cardToPut);

        // Assert - CardSpawned should still be sent to group
        mockGroupProxy.Verify(
            x => x.SendCoreAsync("CardSpawned", It.IsAny<object[]>(), default),
            Times.Once,
            "CardSpawned notification should be sent even if RefreshHand fails"
        );

        // Should log error for RefreshHand failure
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
    }

    #endregion

    #region ArenaService - Tower vision coverage

    [Fact]
    public void ArenaService_GetEnemiesInVision_WithTower_FindsEnemiesInRange()
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

        // Use tower as positioned entity (tower at bottom of map around y=2-3)
        var tower = towers[player1Id][0];

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

        // Place enemy within tower's range (5) - tower is at approximately (8, 2-3)
        var enemyEntity = new TroopEntity(player2Id, enemyPlayerCard, 8, 6)
        {
            Id = Guid.NewGuid(),
            Health = 300
        };

        arena.PlaceEntity(enemyEntity);

        // Act
        var enemiesInVision = service.GetEnemiesInVision(arena, tower).ToList();

        // Assert
        enemiesInVision.Should().Contain(enemyEntity, "Tower should find enemies within its range");
    }

    #endregion
}
