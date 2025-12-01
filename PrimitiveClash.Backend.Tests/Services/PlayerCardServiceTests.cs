using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using PrimitiveClash.Backend.Tests.Infrastructure;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class PlayerCardServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PlayerCardServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region CreateStarterCards Tests

    [Fact]
    public async Task CreateStarterCards_WithAllCardsAvailable_ShouldCreatePlayerCards()
    {
        using var context = _fixture.CreateContext();
        var cardServiceMock = new Mock<ICardService>();
        var service = new PlayerCardService(context, cardServiceMock.Object);

        var userId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var cards = new List<Card>
        {
            new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = "Card1",
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
        };

        cardServiceMock.Setup(x => x.GetInitialCards()).ReturnsAsync(cards);

        var result = await service.CreateStarterCards(userId, deckId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(pc =>
        {
            pc.UserId.Should().Be(userId);
            pc.DeckId.Should().Be(deckId);
            pc.Card.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task CreateStarterCards_WithEmptyCards_ShouldReturnEmptyList()
    {
        using var context = _fixture.CreateContext();
        var cardServiceMock = new Mock<ICardService>();
        var service = new PlayerCardService(context, cardServiceMock.Object);

        var userId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var cards = new List<Card>();

        cardServiceMock.Setup(x => x.GetInitialCards()).ReturnsAsync(cards);

        var result = await service.CreateStarterCards(userId, deckId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}
