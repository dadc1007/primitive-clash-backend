using FluentAssertions;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class DeckTests
{
    private const int MaxDeckSize = 8;

    private static PlayerCard CreateTestPlayerCard(int elixirCost = 3)
    {
        return new PlayerCard
        {
            Id = Guid.NewGuid(),
            CardId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Level = 1,
            Card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = "TestCard",
                ElixirCost = elixirCost,
                Rarity = CardRarity.Common,
                Type = CardType.Troop,
                Damage = 100,
                Targets = [UnitClass.Ground],
                Hp = 300,
                Range = 1,
                HitSpeed = 1.0f,
                MovementSpeed = MovementSpeed.Medium
            }
        };
    }

    #region AddCard Tests

    [Fact]
    public void AddCard_WithAvailableSpace_ShouldAddCardToDeck()
    {
        // Arrange
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        var card = CreateTestPlayerCard();

        // Act
        deck.AddCard(card);

        // Assert
        deck.PlayerCards.Should().Contain(card);
        deck.Size().Should().Be(1);
    }

    [Fact]
    public void AddCard_WhenDeckIsFull_ShouldThrowInvalidDeckSizeException()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };

        for (int i = 0; i < MaxDeckSize; i++)
        {
            deck.AddCard(CreateTestPlayerCard());
        }

        var extraCard = CreateTestPlayerCard();
        var act = () => deck.AddCard(extraCard);

        act.Should().Throw<InvalidDeckSizeException>();
    }

    [Fact]
    public void AddCard_WhenCardAlreadyInDeck_ShouldThrowCardAlreadyInDeckException()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        var card = CreateTestPlayerCard();
        deck.AddCard(card);

        var act = () => deck.AddCard(card);

        act.Should().Throw<CardAlreadyInDeckException>();
    }

    #endregion

    #region RemoveCard Tests

    [Fact]
    public void RemoveCard_WithExistingCard_ShouldRemoveCardFromDeck()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        var card = CreateTestPlayerCard();
        deck.AddCard(card);

        deck.RemoveCard(card);

        deck.PlayerCards.Should().NotContain(card);
        deck.Size().Should().Be(0);
    }

    [Fact]
    public void RemoveCard_WithNonExistentCard_ShouldThrowCardNotInDeckException()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        var card = CreateTestPlayerCard();

        var act = () => deck.RemoveCard(card);

        act.Should().Throw<CardNotInDeckException>();
    }

    #endregion

    #region AverageElixirCost Tests

    [Fact]
    public void AverageElixirCost_WithCards_ShouldReturnCorrectAverage()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        deck.AddCard(CreateTestPlayerCard(2));
        deck.AddCard(CreateTestPlayerCard(4));
        deck.AddCard(CreateTestPlayerCard(6));

        var average = deck.AverageElixirCost();

        average.Should().Be(4.0);
    }

    [Fact]
    public void AverageElixirCost_WithEmptyDeck_ShouldReturnZero()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };

        var average = deck.AverageElixirCost();

        average.Should().Be(0);
    }

    #endregion

    #region Size Tests

    [Fact]
    public void Size_ShouldReturnCorrectCardCount()
    {
        var deck = new Deck(MaxDeckSize) { UserId = Guid.NewGuid() };
        deck.AddCard(CreateTestPlayerCard());
        deck.AddCard(CreateTestPlayerCard());
        deck.AddCard(CreateTestPlayerCard());

        var size = deck.Size();

        size.Should().Be(3);
    }

    #endregion
}
