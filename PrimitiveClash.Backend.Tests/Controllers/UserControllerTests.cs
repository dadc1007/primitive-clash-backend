using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PrimitiveClash.Backend.Controllers;
using PrimitiveClash.Backend.DTOs.User.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Services;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new UserController(_mockUserService.Object);
    }

    [Fact]
    public async Task GetUserMatchStatus_WithUserInMatch_ReturnsOkWithMatchId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        _mockUserService.Setup(s => s.GetMatchId(userId))
            .ReturnsAsync(matchId);

        // Act
        var result = await _controller.GetUserMatchStatus(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<UserMatchStatusResponse>();
        
        var response = okResult.Value as UserMatchStatusResponse;
        response!.UserId.Should().Be(userId);
        response.IsInMatch.Should().BeTrue();
        response.MatchId.Should().Be(matchId);

        _mockUserService.Verify(s => s.GetMatchId(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserMatchStatus_WithUserNotInMatch_ReturnsOkWithoutMatchId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(s => s.GetMatchId(userId))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _controller.GetUserMatchStatus(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<UserMatchStatusResponse>();
        
        var response = okResult.Value as UserMatchStatusResponse;
        response!.UserId.Should().Be(userId);
        response.IsInMatch.Should().BeFalse();
        response.MatchId.Should().BeNull();

        _mockUserService.Verify(s => s.GetMatchId(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserMatchStatus_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserService.Setup(s => s.GetMatchId(userId))
            .ThrowsAsync(new UserNotFoundException(userId));

        // Act
        var result = await _controller.GetUserMatchStatus(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"User with ID {userId} not found.");

        _mockUserService.Verify(s => s.GetMatchId(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserMatchStatus_WithUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database connection failed";

        _mockUserService.Setup(s => s.GetMatchId(userId))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetUserMatchStatus(userId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        _mockUserService.Verify(s => s.GetMatchId(userId), Times.Once);
    }
}
