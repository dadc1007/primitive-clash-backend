using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PrimitiveClash.Backend.Controllers;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Controllers;

public class CardControllerTests
{
    private readonly Mock<ICardService> _mockCardService;
    private readonly CardController _controller;

    public CardControllerTests()
    {
        _mockCardService = new Mock<ICardService>();
        _controller = new CardController(_mockCardService.Object);
    }

    [Fact]
    public async Task GetCardDetails_WithValidCardId_ReturnsOkWithCardResponse()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var troopCard = new TroopCard
        {
            Id = cardId,
            Name = "Archer",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 50,
            Targets = new List<UnitClass> { UnitClass.Ground, UnitClass.Air },
            Hp = 200,
            Range = 5,
            UnitClass = UnitClass.Air,
            VisionRange = 6
        };

        _mockCardService.Setup(s => s.GetCardDetails(cardId))
            .ReturnsAsync(troopCard);

        // Act
        var result = await _controller.GetCardDetails(cardId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        _mockCardService.Verify(s => s.GetCardDetails(cardId), Times.Once);
    }

    [Fact]
    public async Task GetCardDetails_WithNonExistentCard_ReturnsNotFound()
    {
        // Arrange
        var cardId = Guid.NewGuid();

        _mockCardService.Setup(s => s.GetCardDetails(cardId))
            .ThrowsAsync(new CardNotFoundException());

        // Act
        var result = await _controller.GetCardDetails(cardId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult.Value.Should().BeOfType<string>();
    }

    [Fact]
    public async Task GetCardDetails_WhenUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var cardId = Guid.NewGuid();

        _mockCardService.Setup(s => s.GetCardDetails(cardId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCardDetails(cardId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
