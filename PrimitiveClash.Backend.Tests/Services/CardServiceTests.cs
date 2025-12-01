using FluentAssertions;
using Microsoft.Extensions.Options;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Impl;
using PrimitiveClash.Backend.Tests.Infrastructure;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class CardServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public CardServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetInitialCards Tests

    [Fact]
    public async Task GetInitialCards_WithAllStarterCardsInDatabase_ShouldReturnAllCards()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        
        var starterCardNames = new List<string> { "Knight", "Archer", "Giant" };
        var gameSettings = new GameSettings
        {
            StarterCardNames = starterCardNames,
            MaxDeckSize = 8
        };
        var gameSettingsOptions = Options.Create(gameSettings);

        // Add cards to database
        var knight = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        var archer = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Archer",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 50,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 200,
            Range = 5,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Fast,
            ImageUrl = "archer.png"
        };

        var giant = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Giant",
            ElixirCost = 5,
            Rarity = CardRarity.Rare,
            Type = CardType.Troop,
            Damage = 120,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Buildings],
            Hp = 2000,
            Range = 1,
            HitSpeed = 1.5f,
            MovementSpeed = MovementSpeed.Slow,
            ImageUrl = "giant.png"
        };

        context.Cards.AddRange(knight, archer, giant);
        await context.SaveChangesAsync();

        var service = new CardService(context, gameSettingsOptions);

        // Act
        var result = await service.GetInitialCards();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Name == "Knight");
        result.Should().Contain(c => c.Name == "Archer");
        result.Should().Contain(c => c.Name == "Giant");
    }

    [Fact]
    public async Task GetInitialCards_WithMissingStarterCards_ShouldThrowCardsMissingException()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var starterCardNames = new List<string> { "Knight", "Archer", "Giant" };
        var gameSettings = new GameSettings
        {
            StarterCardNames = starterCardNames,
            MaxDeckSize = 8
        };
        var gameSettingsOptions = Options.Create(gameSettings);

        // Only add 2 out of 3 cards
        var knight = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        var archer = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Archer",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 50,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 200,
            Range = 5,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Fast,
            ImageUrl = "archer.png"
        };

        context.Cards.AddRange(knight, archer);
        await context.SaveChangesAsync();

        var service = new CardService(context, gameSettingsOptions);

        // Act
        var act = async () => await service.GetInitialCards();

        // Assert
        await act.Should().ThrowAsync<CardsMissingException>();
    }

    [Fact]
    public async Task GetInitialCards_WithNoCardsInDatabase_ShouldThrowCardsMissingException()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var starterCardNames = new List<string> { "Knight", "Archer", "Giant" };
        var gameSettings = new GameSettings
        {
            StarterCardNames = starterCardNames,
            MaxDeckSize = 8
        };
        var gameSettingsOptions = Options.Create(gameSettings);

        var service = new CardService(context, gameSettingsOptions);

        // Act
        var act = async () => await service.GetInitialCards();

        // Assert
        await act.Should().ThrowAsync<CardsMissingException>();
    }

    #endregion

    #region GetCardDetails Tests

    [Fact]
    public async Task GetCardDetails_WithValidCardId_ShouldReturnCard()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var gameSettings = new GameSettings
        {
            StarterCardNames = [],
            MaxDeckSize = 8
        };
        var gameSettingsOptions = Options.Create(gameSettings);

        var cardId = Guid.NewGuid();
        var knight = new TroopCard
        {
            Id = cardId,
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        context.Cards.Add(knight);
        await context.SaveChangesAsync();

        var service = new CardService(context, gameSettingsOptions);

        // Act
        var result = await service.GetCardDetails(cardId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cardId);
        result.Name.Should().Be("Knight");
        result.ElixirCost.Should().Be(3);
    }

    [Fact]
    public async Task GetCardDetails_WithInvalidCardId_ShouldThrowCardNotFoundException()
    {
        // Arrange
        using var context = _fixture.CreateContext();

        var gameSettings = new GameSettings
        {
            StarterCardNames = [],
            MaxDeckSize = 8
        };
        var gameSettingsOptions = Options.Create(gameSettings);

        var service = new CardService(context, gameSettingsOptions);

        var nonExistentCardId = Guid.NewGuid();

        // Act
        var act = async () => await service.GetCardDetails(nonExistentCardId);

        // Assert
        await act.Should().ThrowAsync<CardNotFoundException>();
    }

    #endregion
}
