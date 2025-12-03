using FluentAssertions;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Tests.Models;

public class PlayerStateBranchTests
{
    [Fact]
    public void GetNextCard_ShouldReturnFifthCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = CreatePlayerCards(userId, 8);
        var playerState = new PlayerState(userId, "TestPlayer", cards);

        // Act
        var nextCard = playerState.GetNextCard();

        // Assert
        nextCard.Should().Be(cards[4], "Fifth card (index 4) should be the next card");
    }

    [Fact]
    public void PlayCard_WithValidCardInHand_ShouldMoveCardToEndOfDeck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = CreatePlayerCards(userId, 8);
        var playerState = new PlayerState(userId, "TestPlayer", cards);
        var cardToPlay = cards[2]; // Third card in hand
        var cardId = cardToPlay.Id;

        // Act
        playerState.PlayCard(cardId);

        // Assert
        playerState.Cards.Should().HaveCount(8);
        playerState.Cards[7].Should().Be(cardToPlay, "Played card should be moved to end of deck");
        playerState.GetHand().Should().NotContain(cardToPlay, "Played card should no longer be in hand");
    }

    [Fact]
    public void PlayCard_WithCardNotInHand_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = CreatePlayerCards(userId, 8);
        var playerState = new PlayerState(userId, "TestPlayer", cards);
        var cardNotInHand = cards[5]; // Card in deck but not in hand

        // Act
        Action act = () => playerState.PlayCard(cardNotInHand.Id);

        // Assert
        act.Should().Throw<CardNotInHandException>("Card is in deck but not in the first 4 positions (hand)");
    }

    [Fact]
    public void PlayCard_WithNonExistentCard_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cards = CreatePlayerCards(userId, 8);
        var playerState = new PlayerState(userId, "TestPlayer", cards);
        var nonExistentCardId = Guid.NewGuid();

        // Act
        Action act = () => playerState.PlayCard(nonExistentCardId);

        // Assert
        act.Should().Throw<CardNotInHandException>("Card doesn't exist in player's deck");
    }

    private static List<PlayerCard> CreatePlayerCards(Guid userId, int count)
    {
        var cards = new List<PlayerCard>();
        
        for (int i = 0; i < count; i++)
        {
            var card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = $"Card {i}",
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
            cards.Add(new PlayerCard 
            { 
                Id = Guid.NewGuid(), 
                Card = card, 
                CardId = cardId,
                UserId = userId,
                Level = 1 
            });
        }

        return cards;
    }
}
