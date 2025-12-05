using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Hubs;

public class GameHubTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly Mock<IBattleService> _battleServiceMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<ILogger<GameHub>> _loggerMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<ISingleClientProxy> _callerProxyMock;
    private readonly Mock<IGroupManager> _groupsMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly GameHub _hub;
    private readonly Dictionary<object, object?> _items;

    public GameHubTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        _battleServiceMock = new Mock<IBattleService>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<GameHub>>();
        _clientsMock = new Mock<IHubCallerClients>();
        _callerProxyMock = new Mock<ISingleClientProxy>();
        _groupsMock = new Mock<IGroupManager>();
        _contextMock = new Mock<HubCallerContext>();
        _items = new Dictionary<object, object?>();

        _contextMock.Setup(x => x.Items).Returns(_items);
        _contextMock.Setup(x => x.ConnectionId).Returns(Guid.NewGuid().ToString());

        _hub = new GameHub(_scopeFactoryMock.Object, _gameServiceMock.Object, _battleServiceMock.Object, _loggerMock.Object)
        {
            Clients = _clientsMock.Object,
            Groups = _groupsMock.Object,
            Context = _contextMock.Object
        };
    }

    private void SetupUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim("oid", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _contextMock.Setup(x => x.User).Returns(claimsPrincipal);
    }

    private Game CreateTestGame(Guid gameId, Guid userId)
    {
        var player2Id = Guid.NewGuid();
        
        // Create cards for player hands
        var card1 = new TroopCard
        {
            Name = "TestCard1",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = new List<UnitClass> { UnitClass.Ground },
            Hp = 100,
            Range = 5,
            DamageArea = 1,
            HitSpeed = 1.5f,
            UnitClass = UnitClass.Ground,
            MovementSpeed = MovementSpeed.Medium,
            VisionRange = 5
        };

        var playerCards = new List<PlayerCard>();
        for (int i = 0; i < 5; i++)
        {
            playerCards.Add(new PlayerCard
            {
                CardId = card1.Id,
                Card = card1,
                UserId = userId,
                Level = 1
            });
        }

        var player2Cards = new List<PlayerCard>();
        for (int i = 0; i < 5; i++)
        {
            player2Cards.Add(new PlayerCard
            {
                CardId = card1.Id,
                Card = card1,
                UserId = player2Id,
                Level = 1
            });
        }

        var playerState1 = new PlayerState(userId, "TestUser1", playerCards);
        var playerState2 = new PlayerState(player2Id, "TestUser2", player2Cards);
        
        var arenaTemplate = new ArenaTemplate { Name = "TestArena" };
        
        var leaderTemplate = new TowerTemplate { Type = TowerType.Leader, Hp = 1000, Damage = 100, Range = 5, Size = 2 };
        var guardianTemplate = new TowerTemplate { Type = TowerType.Guardian, Hp = 800, Damage = 80, Range = 4, Size = 2 };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            { userId, new List<Tower> 
                { 
                    new Tower(userId, leaderTemplate), 
                    new Tower(userId, guardianTemplate), 
                    new Tower(userId, guardianTemplate) 
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
        
        return new Game(gameId, new List<PlayerState> { playerState1, playerState2 }, arena);
    }

    [Fact]
    public async Task JoinGame_ShouldJoinGroupAndNotifyUser_WhenGameExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        SetupUser(userId);

        var game = CreateTestGame(gameId, userId);
        var playerState = game.PlayerStates.First(p => p.Id == userId);

        _gameServiceMock.Setup(s => s.GetGame(gameId)).ReturnsAsync(game);
        _gameServiceMock.Setup(s => s.UpdatePlayerConnectionStatus(gameId, userId, It.IsAny<string>(), true))
            .ReturnsAsync(game);
        _gameServiceMock.Setup(s => s.GetPlayerState(game, userId)).Returns(playerState);
        
        _clientsMock.Setup(c => c.Caller).Returns(_callerProxyMock.Object);
        _clientsMock.Setup(c => c.Client(It.IsAny<string>())).Returns(_callerProxyMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var gameLoopServiceMock = new Mock<IGameLoopService>();

        _scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(s => s.GetService(typeof(IGameLoopService))).Returns(gameLoopServiceMock.Object);

        // Act
        await _hub.JoinGame(gameId);

        // Assert
        _groupsMock.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), gameId.ToString(), default), Times.Once);
        _gameServiceMock.Verify(s => s.UpdatePlayerConnectionStatus(gameId, userId, It.IsAny<string>(), true), Times.Once);
        _callerProxyMock.Verify(c => c.SendCoreAsync("JoinedToGame", It.IsAny<object[]>(), default), Times.Once);
        
        Assert.True(_items.ContainsKey("SessionId"));
        Assert.Equal(gameId, _items["SessionId"]);
        Assert.True(_items.ContainsKey("UserId"));
        Assert.Equal(userId, _items["UserId"]);
    }

    [Fact]
    public async Task JoinGame_ShouldSendError_WhenUserNotInGame()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        SetupUser(userId);

        var game = CreateTestGame(gameId, otherUserId); // Game with different user

        _gameServiceMock.Setup(s => s.GetGame(gameId)).ReturnsAsync(game);
        _clientsMock.Setup(c => c.Caller).Returns(_callerProxyMock.Object);

        // Act
        await _hub.JoinGame(gameId);

        // Assert
        _callerProxyMock.Verify(c => c.SendCoreAsync("Error", 
            It.Is<object[]>(o => o[0].ToString() == "You are not authorized to join this game session."), 
            default), Times.Once);
        _groupsMock.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task SpawnCard_ShouldCallBattleService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var x = 10;
        var y = 20;
        
        SetupUser(userId);
        _items["SessionId"] = gameId;

        // Act
        await _hub.SpawnCard(gameId, cardId, x, y);

        // Assert
        _battleServiceMock.Verify(s => s.SpawnCard(gameId, userId, cardId, x, y), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldUpdateConnectionStatus_WhenUserIsInGame()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        _items["UserId"] = userId;
        _items["SessionId"] = gameId;

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _gameServiceMock.Verify(s => s.UpdatePlayerConnectionStatus(gameId, userId, null, false), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldDoNothing_WhenUserNotInGame()
    {
        // Arrange
        // No items in context

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _gameServiceMock.Verify(s => s.UpdatePlayerConnectionStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
}
