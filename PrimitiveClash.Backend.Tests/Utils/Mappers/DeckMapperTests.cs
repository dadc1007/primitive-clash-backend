using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class DeckMapperTests
{
    [Fact]
    public void ToDeckResponse_WithValidDeck_ReturnsMappedResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var card1 = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Archer",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "archer.png"
        };
        var card2 = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 4,
            Rarity = CardRarity.Rare,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "knight.png"
        };

        var playerCard1 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = card1.Id,
            Card = card1,
            Level = 5
        };
        var playerCard2 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = card2.Id,
            Card = card2,
            Level = 3
        };

        var deck = new Deck(8)
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = new List<PlayerCard> { playerCard1, playerCard2 }
        };

        // Act
        var result = deck.ToDeckResponse();

        // Assert
        result.Should().NotBeNull();
        result.DeckId.Should().Be(deck.Id);
        result.Size.Should().Be(2);
        result.AverageElixirCost.Should().Be(3.5);
        result.Cards.Should().HaveCount(2);
        result.Cards[0].PlayerCardId.Should().Be(playerCard1.Id);
        result.Cards[0].CardId.Should().Be(card1.Id);
        result.Cards[0].CardName.Should().Be("Archer");
        result.Cards[0].Level.Should().Be(5);
        result.Cards[1].PlayerCardId.Should().Be(playerCard2.Id);
        result.Cards[1].CardId.Should().Be(card2.Id);
        result.Cards[1].CardName.Should().Be("Knight");
        result.Cards[1].Level.Should().Be(3);
    }

    [Fact]
    public void ToDeckResponse_WithNullDeck_ThrowsInvalidOperationException()
    {
        // Arrange
        Deck? deck = null;

        // Act
        var act = () => deck!.ToDeckResponse();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deck is null");
    }

    [Fact]
    public void ToDeckResponse_WithEmptyDeck_ReturnsResponseWithZeroSize()
    {
        // Arrange
        var deck = new Deck(8)
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PlayerCards = new List<PlayerCard>()
        };

        // Act
        var result = deck.ToDeckResponse();

        // Assert
        result.Size.Should().Be(0);
        result.AverageElixirCost.Should().Be(0);
        result.Cards.Should().BeEmpty();
    }

    [Fact]
    public void ToCardInDeckResponse_WithValidPlayerCard_ReturnsMappedResponse()
    {
        // Arrange
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Goblin",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "goblin.png"
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CardId = card.Id,
            Card = card,
            Level = 10
        };

        // Act
        var result = playerCard.ToCardInDeckResponse();

        // Assert
        result.Should().NotBeNull();
        result.PlayerCardId.Should().Be(playerCard.Id);
        result.CardId.Should().Be(card.Id);
        result.CardName.Should().Be("Goblin");
        result.Rarity.Should().Be(CardRarity.Common);
        result.ElixirCost.Should().Be(2);
        result.Level.Should().Be(10);
        result.ImageUrl.Should().Be("goblin.png");
    }

    [Fact]
    public void ToCardInDeckResponse_WithNullPlayerCard_ThrowsInvalidOperationException()
    {
        // Arrange
        PlayerCard? playerCard = null;

        // Act
        var act = () => playerCard!.ToCardInDeckResponse();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PlayerCard or its Card property is null");
    }

    [Fact]
    public void ToDeckResponse_WithMultipleCards_CalculatesCorrectAverage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = new List<PlayerCard>();

        for (int i = 1; i <= 8; i++)
        {
            var card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = $"Card{i}",
                ElixirCost = i,
                Rarity = CardRarity.Common,
                Targets = new List<UnitClass> { UnitClass.Ground },
                ImageUrl = $"card{i}.png"
            };

            var playerCard = new PlayerCard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CardId = card.Id,
                Card = card,
                Level = 1
            };

            cards.Add(playerCard);
        }

        var deck = new Deck(8)
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = cards
        };

        // Act
        var result = deck.ToDeckResponse();

        // Assert
        result.Size.Should().Be(8);
        result.AverageElixirCost.Should().Be(4.5); // (1+2+3+4+5+6+7+8)/8 = 4.5
        result.Cards.Should().HaveCount(8);
    }

    [Fact]
    public void ToDeckResponse_WithDifferentRarities_MapsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var legendaryCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Princess",
            ElixirCost = 3,
            Rarity = CardRarity.Legendary,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "princess.png"
        };
        var epicCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Witch",
            ElixirCost = 5,
            Rarity = CardRarity.Epic,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "witch.png"
        };

        var playerCard1 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = legendaryCard.Id,
            Card = legendaryCard,
            Level = 1
        };
        var playerCard2 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = epicCard.Id,
            Card = epicCard,
            Level = 2
        };

        var deck = new Deck(8)
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = new List<PlayerCard> { playerCard1, playerCard2 }
        };

        // Act
        var result = deck.ToDeckResponse();

        // Assert
        result.Cards[0].Rarity.Should().Be(CardRarity.Legendary);
        result.Cards[1].Rarity.Should().Be(CardRarity.Epic);
    }
}
