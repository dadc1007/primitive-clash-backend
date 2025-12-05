using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using PrimitiveClash.Backend.Tests.Infrastructure;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class UserServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetOrCreateUser_ExistingUser_ReturnsUser()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var oid = userId.ToString();
        var email = "test@example.com";

        var existingUser = new User
        {
            Id = userId,
            Username = "test",
            Email = email
        };

        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var result = await service.GetOrCreateUser(oid, email);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(email);
        result.Username.Should().Be("test");

        deckServiceMock.Verify(d => d.InitializeDeck(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUser_NewUser_CreatesUserWithUsernameFromEmail()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var oid = Guid.NewGuid().ToString();
        var email = "newuser@example.com";
        var expectedUsername = "newuser";

        var result = await service.GetOrCreateUser(oid, email);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Username.Should().Be(expectedUsername);
        result.Id.ToString().Should().Be(oid);

        deckServiceMock.Verify(d => d.InitializeDeck(It.Is<Guid>(id => id.ToString() == oid)), Times.Once);

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be(expectedUsername);
    }

    [Fact]
    public async Task GetOrCreateUser_NewUserWithComplexEmail_ExtractsCorrectUsername()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var oid = Guid.NewGuid().ToString();
        var email = "john.doe+test@company.co.uk";
        var expectedUsername = "john.doe+test";

        var result = await service.GetOrCreateUser(oid, email);

        result.Username.Should().Be(expectedUsername);
    }

    [Fact]
    public async Task GetOrCreateUser_NewUser_InitializesDeck()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var oid = userId.ToString();
        var email = "test@example.com";

        await service.GetOrCreateUser(oid, email);

        deckServiceMock.Verify(d => d.InitializeDeck(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserName_ExistingUser_ReturnsUsername()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var expectedUsername = "testuser";

        var user = new User
        {
            Id = userId,
            Username = expectedUsername,
            Email = "testuser@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.GetUserName(userId);

        result.Should().Be(expectedUsername);
    }

    [Fact]
    public async Task GetUserName_NonExistingUser_ThrowsUserNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.GetUserName(userId));
    }

    [Fact]
    public async Task UpdateUserMatchId_ExistingUser_UpdatesMatchId()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "testuser@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await service.UpdateUserMatchId(userId, matchId);

        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.MatchId.Should().Be(matchId);
    }

    [Fact]
    public async Task UpdateUserMatchId_WithNullMatchId_ClearsMatchId()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var initialMatchId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "testuser@example.com",
            MatchId = initialMatchId
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await service.UpdateUserMatchId(userId, null);

        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.MatchId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserMatchId_NonExistingUser_ThrowsUserNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.UpdateUserMatchId(userId, matchId));
    }

    [Fact]
    public async Task GetMatchId_ExistingUserWithMatchId_ReturnsMatchId()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "testuser@example.com",
            MatchId = matchId
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.GetMatchId(userId);

        result.Should().Be(matchId);
    }

    [Fact]
    public async Task GetMatchId_ExistingUserWithoutMatchId_ReturnsNull()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "testuser@example.com",
            MatchId = null
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await service.GetMatchId(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMatchId_NonExistingUser_ThrowsUserNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.GetMatchId(userId));
    }

    [Fact]
    public async Task GetOrCreateUser_NewUser_SavesUserToDatabase()
    {
        using var context = _fixture.CreateContext();
        var deckServiceMock = new Mock<IDeckService>();
        var service = new UserService(context, deckServiceMock.Object);

        var userId = Guid.NewGuid();
        var oid = userId.ToString();
        var email = "persistent@example.com";

        var deck = new Deck { UserId = userId, PlayerCards = new List<PlayerCard>() };
        deckServiceMock.Setup(d => d.InitializeDeck(userId)).ReturnsAsync(deck);

        var result = await service.GetOrCreateUser(oid, email);

        // Verify the user was created
        result.Should().NotBeNull();
        result.Id.ToString().Should().Be(oid);
        result.Email.Should().Be(email);

        // Verify the user exists in database
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        savedUser.Should().NotBeNull();
        savedUser!.Id.ToString().Should().Be(oid);
    }
}
