using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

public class NotificationServiceNullCardTests
{
    [Fact]
    public async Task NotifyCardSpawned_WithNullConnectionId_LogsAndCallsRefreshHandWithNullCheck()
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

        var service = new NotificationService(mockHubContext.Object, mockLogger.Object);

        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Type = CardType.Troop,
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Damage = 50,
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

        var playerCard = new PlayerCard
        {
            CardId = card.Id,
            UserId = userId,
            Card = card
        };

        var entity = new TroopEntity(userId, playerCard, 5, 5);
        
        // Player WITHOUT ConnectionId - this triggers the if branch in NotifyRefreshHand
        var player = new PlayerState(userId, new List<PlayerCard> { playerCard });
        // ConnectionId is null by default

        // Act
        await service.NotifyCardSpawned(sessionId, player, entity, playerCard);

        // Assert
        mockGroupProxy.Verify(x => x.SendCoreAsync(
            "CardSpawned",
            It.IsAny<object[]>(),
            default), Times.Once);

        // Verify the warning was logged when ConnectionId was null
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("sin ConnectionId")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
