// using FluentAssertions;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using PrimitiveClash.Backend.Controllers;
// using PrimitiveClash.Backend.DTOs.Deck.Responses;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Services;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Controllers;

// public class DeckControllerTests
// {
//     private readonly Mock<IDeckService> _deckServiceMock;
//     private readonly DeckController _controller;

//     public DeckControllerTests()
//     {
//         _deckServiceMock = new Mock<IDeckService>();
//         _controller = new DeckController(_deckServiceMock.Object);
//     }

//     #region GetDeckByUserId Tests

//     [Fact]
//     public async Task GetDeckByUserId_WithValidUserId_ShouldReturnOkResult()
//     {
//         // Arrange
//         var userId = Guid.NewGuid();
//         var expectedDeck = new Deck
//         {
//             Id = Guid.NewGuid(),
//             UserId = userId,
//             PlayerCards = new List<PlayerCard>()
//         };

//         _deckServiceMock
//             .Setup(x => x.GetDeckByUserId(userId))
//             .ReturnsAsync(expectedDeck);

//         // Act
//         var result = await _controller.GetDeckByUserId(userId);

//         // Assert
//         result.Should().BeOfType<OkObjectResult>();
//         var okResult = result as OkObjectResult;

//         var response = okResult!.Value as DeckResponse;
//         response.Should().NotBeNull();
//         response!.DeckId.Should().Be(expectedDeck.Id);

//         _deckServiceMock.Verify(
//             x => x.GetDeckByUserId(userId),
//             Times.Once);
//     }

//     [Fact]
//     public async Task GetDeckByUserId_WithNonExistentUser_ShouldReturnNotFound()
//     {
//         var userId = Guid.NewGuid();

//         _deckServiceMock
//             .Setup(x => x.GetDeckByUserId(userId))
//             .ThrowsAsync(new DeckNotFoundException(userId));

//         var result = await _controller.GetDeckByUserId(userId);

//         result.Should().BeOfType<NotFoundObjectResult>();
//         var notFoundResult = result as NotFoundObjectResult;
//         notFoundResult!.Value.Should().NotBeNull();
//         notFoundResult.Value.Should().BeOfType<string>();
//         notFoundResult.Value.ToString()!.Should().Contain(userId.ToString());
//     }

//     [Fact]
//     public async Task GetDeckByUserId_WithEmptyDeck_ShouldReturnOkWithEmptyCards()
//     {
//         var userId = Guid.NewGuid();
//         var emptyDeck = new Deck
//         {
//             Id = Guid.NewGuid(),
//             UserId = userId,
//             PlayerCards = new List<PlayerCard>()
//         };

//         _deckServiceMock
//             .Setup(x => x.GetDeckByUserId(userId))
//             .ReturnsAsync(emptyDeck);

//         var result = await _controller.GetDeckByUserId(userId);

//         result.Should().BeOfType<OkObjectResult>();
//         var okResult = result as OkObjectResult;

//         var response = okResult!.Value as DeckResponse;
//         response.Should().NotBeNull();
//         response!.Cards.Should().BeEmpty();
//     }

//     [Fact]
//     public async Task GetDeckByUserId_WithUnexpectedException_ShouldReturnInternalServerError()
//     {
//         var userId = Guid.NewGuid();

//         _deckServiceMock
//             .Setup(x => x.GetDeckByUserId(userId))
//             .ThrowsAsync(new Exception("Database connection failed"));

//         var result = await _controller.GetDeckByUserId(userId);

//         result.Should().BeOfType<ObjectResult>();
//         var objectResult = result as ObjectResult;
//         objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
//     }

//     [Theory]
//     [InlineData("00000000-0000-0000-0000-000000000001")]
//     [InlineData("00000000-0000-0000-0000-000000000002")]
//     [InlineData("00000000-0000-0000-0000-000000000003")]
//     public async Task GetDeckByUserId_WithDifferentUserIds_ShouldCallServiceWithCorrectId(string userIdString)
//     {
//         var userId = Guid.Parse(userIdString);
//         var deck = new Deck
//         {
//             Id = Guid.NewGuid(),
//             UserId = userId,
//             PlayerCards = new List<PlayerCard>()
//         };

//         _deckServiceMock
//             .Setup(x => x.GetDeckByUserId(userId))
//             .ReturnsAsync(deck);

//         var result = await _controller.GetDeckByUserId(userId);

//         result.Should().BeOfType<OkObjectResult>();
//         _deckServiceMock.Verify(
//             x => x.GetDeckByUserId(userId),
//             Times.Once);
//     }

//     #endregion
// }
