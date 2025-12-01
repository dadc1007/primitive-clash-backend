using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using PrimitiveClash.Backend.Tests.Infrastructure;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class DeckServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly int _maxDeckSize = 8;

    public DeckServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region InitializeDeck Tests

    [Fact]
    public async Task InitializeDeck_WithValidUserId_ShouldCreateDeckWithStarterCards()
    {
        using var context = _fixture.CreateContext();
        var playerCardServiceMock = new Mock<IPlayerCardService>();
        var gameSettings = new GameSettings 
        { 
            MaxDeckSize = _maxDeckSize,
            StarterCardNames = []
        };
        var gameSettingsOptions = Options.Create(gameSettings);
        var service = new DeckService(context, playerCardServiceMock.Object, gameSettingsOptions);

        var userId = Guid.NewGuid();
        var starterCards = Enumerable.Range(0, _maxDeckSize)
            .Select(_ => new PlayerCard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CardId = Guid.NewGuid(),
                Level = 1,
                Card = new TroopCard
                {
                    Id = Guid.NewGuid(),
                    Name = "TestCard",
                    ElixirCost = 3,
                    Rarity = CardRarity.Common,
                    Type = CardType.Troop,
                    Damage = 100,
                    UnitClass = UnitClass.Ground,
                    Targets = [UnitClass.Ground],
                    Hp = 300,
                    Range = 1,
                    HitSpeed = 1.0f,
                    MovementSpeed = MovementSpeed.Medium,
                    ImageUrl = "test.png"
                }
            })
            .ToList();

        playerCardServiceMock
            .Setup(x => x.CreateStarterCards(userId, It.IsAny<Guid>()))
            .ReturnsAsync(starterCards);

        var result = await service.InitializeDeck(userId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.PlayerCards.Should().HaveCount(_maxDeckSize);
        result.Size().Should().Be(_maxDeckSize);

        playerCardServiceMock.Verify(
            x => x.CreateStarterCards(userId, It.IsAny<Guid>()), 
            Times.Once);
    }

    [Fact]
    public async Task InitializeDeck_WithInvalidCardCount_ShouldThrowInvalidDeckSizeException()
    {
        using var context = _fixture.CreateContext();
        var playerCardServiceMock = new Mock<IPlayerCardService>();
        var gameSettings = new GameSettings 
        { 
            MaxDeckSize = _maxDeckSize,
            StarterCardNames = []
        };
        var gameSettingsOptions = Options.Create(gameSettings);
        var service = new DeckService(context, playerCardServiceMock.Object, gameSettingsOptions);

        var userId = Guid.NewGuid();
        var invalidCards = new List<PlayerCard>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                CardId = Guid.NewGuid(), 
                Level = 1,
                Card = new TroopCard
                {
                    Id = Guid.NewGuid(),
                    Name = "TestCard",
                    ElixirCost = 3,
                    Rarity = CardRarity.Common,
                    Type = CardType.Troop,
                    Damage = 100,
                    UnitClass = UnitClass.Ground,
                    Targets = [UnitClass.Ground],
                    Hp = 300,
                    Range = 1,
                    HitSpeed = 1.0f,
                    MovementSpeed = MovementSpeed.Medium,
                    ImageUrl = "test.png"
                }
            }
        };

        playerCardServiceMock
            .Setup(x => x.CreateStarterCards(userId, It.IsAny<Guid>()))
            .ReturnsAsync(invalidCards);

        var act = async () => await service.InitializeDeck(userId);

        await act.Should().ThrowAsync<InvalidDeckSizeException>();
    }

    #endregion

    #region GetDeckByUserId Tests

    [Fact]
    public async Task GetDeckByUserId_WithExistingDeck_ShouldReturnDeckWithCards()
    {
        using var context = _fixture.CreateContext();
        var playerCardServiceMock = new Mock<IPlayerCardService>();
        var gameSettings = new GameSettings 
        { 
            MaxDeckSize = _maxDeckSize,
            StarterCardNames = []
        };
        var gameSettingsOptions = Options.Create(gameSettings);
        var service = new DeckService(context, playerCardServiceMock.Object, gameSettingsOptions);

        var userId = Guid.NewGuid();

        // Crear el usuario primero para satisfacer la foreign key
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var deck = new Deck(_maxDeckSize)
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = new List<PlayerCard>()
        };

        context.Decks.Add(deck);
        await context.SaveChangesAsync();

        var result = await service.GetDeckByUserId(userId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Id.Should().Be(deck.Id);
    }

    [Fact]
    public async Task GetDeckByUserId_WithNonExistentUser_ShouldThrowDeckNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var playerCardServiceMock = new Mock<IPlayerCardService>();
        var gameSettings = new GameSettings 
        { 
            MaxDeckSize = _maxDeckSize,
            StarterCardNames = []
        };
        var gameSettingsOptions = Options.Create(gameSettings);
        var service = new DeckService(context, playerCardServiceMock.Object, gameSettingsOptions);

        var userId = Guid.NewGuid();

        var act = async () => await service.GetDeckByUserId(userId);

        await act.Should().ThrowAsync<DeckNotFoundException>();
    }

    #endregion
}
