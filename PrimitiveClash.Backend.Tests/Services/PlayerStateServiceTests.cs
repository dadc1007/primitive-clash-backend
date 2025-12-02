using FluentAssertions;
using Moq;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class PlayerStateServiceTests
{
    private readonly Mock<IDeckService> _mockDeckService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly PlayerStateService _playerStateService;

    public PlayerStateServiceTests()
    {
        _mockDeckService = new Mock<IDeckService>();
        _mockUserService = new Mock<IUserService>();
        _playerStateService = new PlayerStateService(_mockDeckService.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task CreatePlayerState_ShouldReturnPlayerStateWithShuffledCards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cardId1 = Guid.NewGuid();
        var cardId2 = Guid.NewGuid();
        var cardId3 = Guid.NewGuid();
        var cardId4 = Guid.NewGuid();
        var cardId5 = Guid.NewGuid();

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = new List<PlayerCard>
            {
                new PlayerCard { Id = Guid.NewGuid(), UserId = userId, CardId = cardId1, Card = new TroopCard { Id = cardId1, Name = "Card1", Targets = new List<UnitClass>(), ImageUrl = "" } },
                new PlayerCard { Id = Guid.NewGuid(), UserId = userId, CardId = cardId2, Card = new TroopCard { Id = cardId2, Name = "Card2", Targets = new List<UnitClass>(), ImageUrl = "" } },
                new PlayerCard { Id = Guid.NewGuid(), UserId = userId, CardId = cardId3, Card = new TroopCard { Id = cardId3, Name = "Card3", Targets = new List<UnitClass>(), ImageUrl = "" } },
                new PlayerCard { Id = Guid.NewGuid(), UserId = userId, CardId = cardId4, Card = new TroopCard { Id = cardId4, Name = "Card4", Targets = new List<UnitClass>(), ImageUrl = "" } },
                new PlayerCard { Id = Guid.NewGuid(), UserId = userId, CardId = cardId5, Card = new TroopCard { Id = cardId5, Name = "Card5", Targets = new List<UnitClass>(), ImageUrl = "" } }
            }
        };

        _mockDeckService
            .Setup(x => x.GetDeckByUserId(userId))
            .ReturnsAsync(deck);

        // Act
        var result = await _playerStateService.CreatePlayerState(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Cards.Should().HaveCount(5);
        result.CurrentElixir.Should().Be(5); // Default initial elixir
        result.GetHand().Should().HaveCount(4); // Initial hand size
        
        // Verify all cards are from the original deck
        var originalCardIds = deck.PlayerCards.Select(pc => pc.Id).ToList();
        result.Cards.Select(pc => pc.Id).Should().BeSubsetOf(originalCardIds);
    }

    [Fact]
    public async Task CreatePlayerState_WithMultipleCalls_ShouldProduceDifferentCardOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCards = Enumerable.Range(1, 20)
            .Select(i =>
            {
                var cardId = Guid.NewGuid();
                return new PlayerCard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CardId = cardId,
                    Card = new TroopCard
                    {
                        Id = cardId,
                        Name = $"Card{i}",
                        Targets = new List<UnitClass>(),
                        ImageUrl = ""
                    }
                };
            })
            .ToList();

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlayerCards = playerCards
        };

        _mockDeckService
            .Setup(x => x.GetDeckByUserId(userId))
            .ReturnsAsync(deck);

        // Act - Create multiple player states
        var results = new List<PlayerState>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _playerStateService.CreatePlayerState(userId));
        }

        // Assert - At least one should have a different order
        // (with 20 cards, the probability of all 10 being identical is astronomically small)
        var firstOrder = string.Join(",", results[0].Cards.Select(pc => pc.Id));
        var allSame = results.All(r => string.Join(",", r.Cards.Select(pc => pc.Id)) == firstOrder);
        
        allSame.Should().BeFalse("shuffling should produce different card orders");
    }
}
