using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class RefreshHandNotificationMapperTests
{
    private PlayerCard CreateTestPlayerCard(string cardName, int elixirCost)
    {
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = cardName,
            ElixirCost = elixirCost,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            Targets = new List<UnitClass> { UnitClass.Ground },
            Hp = 500,
            Range = 1,
            UnitClass = UnitClass.Ground,
            VisionRange = 5,
            ImageUrl = $"https://example.com/{cardName.ToLower()}.png"
        };

        return new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CardId = card.Id,
            Card = card,
            Level = 1
        };
    }

    [Fact]
    public void ToRefreshHandNotification_WithValidCards_ReturnsCorrectNotification()
    {
        // Arrange
        var cardToPut = CreateTestPlayerCard("Knight", 3);
        var nextCard = CreateTestPlayerCard("Archer", 3);
        var elixir = 7.5m;

        // Act
        var result = RefreshHandNotificationMapper.ToRefreshHandNotification(cardToPut, nextCard, elixir);

        // Assert
        result.Should().NotBeNull();
        result.Elixir.Should().Be(elixir);
        
        result.CardToPut.Should().NotBeNull();
        result.CardToPut.PlayerId.Should().Be(cardToPut.UserId);
        result.CardToPut.PlayerCardId.Should().Be(cardToPut.Id);
        result.CardToPut.CardId.Should().Be(cardToPut.Card.Id);
        result.CardToPut.Elixir.Should().Be(3);
        result.CardToPut.ImageUrl.Should().Be("https://example.com/knight.png");

        result.NextCard.Should().NotBeNull();
        result.NextCard.PlayerId.Should().Be(nextCard.UserId);
        result.NextCard.PlayerCardId.Should().Be(nextCard.Id);
        result.NextCard.CardId.Should().Be(nextCard.Card.Id);
        result.NextCard.Elixir.Should().Be(3);
        result.NextCard.ImageUrl.Should().Be("https://example.com/archer.png");
    }

    [Fact]
    public void ToRefreshHandNotification_WithDifferentElixirValues_MapsCorrectly()
    {
        // Arrange
        var cardToPut = CreateTestPlayerCard("Giant", 5);
        var nextCard = CreateTestPlayerCard("Goblin", 2);

        // Act
        var result1 = RefreshHandNotificationMapper.ToRefreshHandNotification(cardToPut, nextCard, 0m);
        var result2 = RefreshHandNotificationMapper.ToRefreshHandNotification(cardToPut, nextCard, 10m);

        // Assert
        result1.Elixir.Should().Be(0m);
        result2.Elixir.Should().Be(10m);
    }
}
