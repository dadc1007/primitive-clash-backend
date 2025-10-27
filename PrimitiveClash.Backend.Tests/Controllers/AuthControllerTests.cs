using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PrimitiveClash.Backend.Controllers;
using PrimitiveClash.Backend.DTOs.Auth.Requests;
using PrimitiveClash.Backend.DTOs.Auth.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    #region SignUp Tests

    [Fact]
    public async Task SignUp_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new SignupRequest("testuser", "test@example.com", "SecurePassword123!");

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = "hashedPassword123"
        };

        _authServiceMock
            .Setup(x => x.RegisterUser(request.Username, request.Email, request.Password))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.SignUp(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);
        
        var response = objectResult.Value as AuthSuccessResponse;
        response.Should().NotBeNull();
        response!.Username.Should().Be(request.Username);
        response.Email.Should().Be(request.Email);

        _authServiceMock.Verify(
            x => x.RegisterUser(request.Username, request.Email, request.Password),
            Times.Once);
    }

    [Fact]
    public async Task SignUp_WithExistingUsername_ShouldReturnConflict()
    {
        var request = new SignupRequest("existinguser", "newemail@example.com", "SecurePassword123!");

        _authServiceMock
            .Setup(x => x.RegisterUser(request.Username, request.Email, request.Password))
            .ThrowsAsync(new UsernameExistsException("Username already exists"));

        var result = await _controller.SignUp(request);

        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SignUp_WithExistingEmail_ShouldReturnConflict()
    {
        var request = new SignupRequest("newuser", "existing@example.com", "SecurePassword123!");

        _authServiceMock
            .Setup(x => x.RegisterUser(request.Username, request.Email, request.Password))
            .ThrowsAsync(new EmailExistsException("Email already exists"));

        var result = await _controller.SignUp(request);

        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SignUp_WithUnexpectedException_ShouldReturnInternalServerError()
    {
        var request = new SignupRequest("testuser", "test@example.com", "SecurePassword123!");

        _authServiceMock
            .Setup(x => x.RegisterUser(request.Username, request.Email, request.Password))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.SignUp(request);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkResult()
    {
        var request = new LoginRequest("test@example.com", "SecurePassword123!");

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = request.Email,
            PasswordHash = "hashedPassword123"
        };

        _authServiceMock
            .Setup(x => x.LoginUser(request.Email, request.Password))
            .ReturnsAsync(expectedUser);

        var result = await _controller.Login(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        
        var response = okResult!.Value as AuthSuccessResponse;
        response.Should().NotBeNull();
        response!.Email.Should().Be(request.Email);

        _authServiceMock.Verify(
            x => x.LoginUser(request.Email, request.Password),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var request = new LoginRequest("test@example.com", "WrongPassword");

        _authServiceMock
            .Setup(x => x.LoginUser(request.Email, request.Password))
            .ThrowsAsync(new InvalidCredentialsException());

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithUnexpectedException_ShouldReturnInternalServerError()
    {
        var request = new LoginRequest("test@example.com", "SecurePassword123!");

        _authServiceMock
            .Setup(x => x.LoginUser(request.Email, request.Password))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.Login(request);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Theory]
    [InlineData("user1@example.com", "Password123")]
    [InlineData("user2@example.com", "Password456")]
    [InlineData("user3@example.com", "Password789")]
    public async Task Login_WithMultipleValidUsers_ShouldReturnOkResult(string email, string password)
    {
        var request = new LoginRequest(email, password);

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Username = email.Split('@')[0],
            Email = email,
            PasswordHash = "hashedPassword"
        };

        _authServiceMock
            .Setup(x => x.LoginUser(email, password))
            .ReturnsAsync(expectedUser);

        var result = await _controller.Login(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthSuccessResponse;
        response!.Email.Should().Be(email);
    }

    #endregion
}
