using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PrimitiveClash.Backend.Data;
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
}
