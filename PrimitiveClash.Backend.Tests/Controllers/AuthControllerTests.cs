using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PrimitiveClash.Backend.Controllers;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;
using System.Security.Claims;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new AuthController(_mockUserService.Object);
    }

    [Fact]
    public async Task UpsertUser_WithValidToken_ReturnsOkWithUserData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oid = userId.ToString();
        var email = "test@example.com";

        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("preferred_username", email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var user = new User
        {
            Id = userId,
            Username = "test",
            Email = email
        };

        _mockUserService.Setup(s => s.GetOrCreateUser(oid, email))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        _mockUserService.Verify(s => s.GetOrCreateUser(oid, email), Times.Once);
    }

    [Fact]
    public async Task UpsertUser_WithMissingOid_ReturnsBadRequest()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("preferred_username", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
        
        _mockUserService.Verify(s => s.GetOrCreateUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpsertUser_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim("oid", oid)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
        
        _mockUserService.Verify(s => s.GetOrCreateUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpsertUser_WithEmptyOid_ReturnsBadRequest()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("oid", ""),
            new Claim("preferred_username", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpsertUser_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("preferred_username", "")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpsertUser_WhenUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var email = "test@example.com";

        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("preferred_username", email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockUserService.Setup(s => s.GetOrCreateUser(oid, email))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpsertUser_WhenUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var email = "test@example.com";

        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("preferred_username", email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockUserService.Setup(s => s.GetOrCreateUser(oid, email))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.UpsertUser();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
