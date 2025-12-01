using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class PlayerHandNotificationMapperTests
{
    private Arena CreateTestArena()
    {
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
            Hp = 2000,
            Damage = 100,
            Range = 7,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 1500,
            Damage = 80,
            Range = 6,
            Size = 3
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            {
                player1Id,
                new List<Tower>
                {
                    new Tower(player1Id, leaderTemplate),
                    new Tower(player1Id, guardianTemplate),
                    new Tower(player1Id, guardianTemplate)
                }
            },
            {
                player2Id,
                new List<Tower>
                {
                    new Tower(player2Id, leaderTemplate),
                    new Tower(player2Id, guardianTemplate),
                    new Tower(player2Id, guardianTemplate)
                }
            }
        };

        return new Arena(arenaTemplate, towers);
    }

    private List<PlayerCard> CreateTestPlayerCards(Guid userId, int count)
    {
        var cards = new List<PlayerCard>();
        
        for (int i = 0; i < count; i++)
        {
            var card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = $"Card{i}",
                ElixirCost = 3,
                Rarity = CardRarity.Common,
                Type = CardType.Troop,
                Damage = 100,
                Targets = new List<UnitClass> { UnitClass.Ground },
                Hp = 500,
                Range = 1,
                UnitClass = UnitClass.Ground,
                VisionRange = 5,
                ImageUrl = $"https://example.com/card{i}.png"
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

        return cards;
    }

    [Fact]
    public void ToPlayerHandNotification_WithValidPlayerState_ReturnsCorrectNotification()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestPlayer",
            Email = "test@example.com"
        };

        var playerCards = CreateTestPlayerCards(user.Id, 9); // 4 en mano + 5 en cola
        var playerState = new PlayerState(user.Id, playerCards);

        var arena = CreateTestArena();

        // Act
        var result = PlayerHandNotificationMapper.ToPlayerHandNotification(arena, playerState);

        // Assert
        result.Should().NotBeNull();
        result.Hand.Should().HaveCount(4); // Mano tiene 4 cartas
        result.NextCard.Should().NotBeNull();
        result.NextCard.PlayerId.Should().Be(user.Id);
    }

    [Fact]
    public void ToPlayerHandNotification_HandCards_HaveCorrectProperties()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestPlayer",
            Email = "test@example.com"
        };

        var playerCards = CreateTestPlayerCards(user.Id, 9);
        var playerState = new PlayerState(user.Id, playerCards);

        var arena = CreateTestArena();

        // Act
        var result = PlayerHandNotificationMapper.ToPlayerHandNotification(arena, playerState);

        // Assert
        foreach (var handCard in result.Hand)
        {
            handCard.PlayerId.Should().Be(user.Id);
            handCard.Elixir.Should().Be(3);
            handCard.ImageUrl.Should().StartWith("https://example.com/");
        }
    }
}
