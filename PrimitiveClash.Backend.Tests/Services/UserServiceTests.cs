// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using PrimitiveClash.Backend.Data;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Services;
// using PrimitiveClash.Backend.Services.Impl;
// using PrimitiveClash.Backend.Tests.Infrastructure;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Services;

// public class UserServiceTests : IClassFixture<DatabaseFixture>
// {
//     private readonly DatabaseFixture _fixture;

//     public UserServiceTests(DatabaseFixture fixture)
//     {
//         _fixture = fixture;
//     }

//     #region RegisterUser Tests

//     [Fact]
//     public async Task RegisterUser_WithValidData_ShouldCreateUserAndDeck()
//     {
//         using var context = _fixture.CreateContext();
//         var deckServiceMock = new Mock<IDeckService>();
//         var service = new UserService(context, deckServiceMock.Object);

//         var username = "newuser";
//         var email = "new@example.com";
//         var password = "password123";
//         var mockDeck = new Deck(8) { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };

//         deckServiceMock
//             .Setup(x => x.InitializeDeck(It.IsAny<Guid>()))
//             .ReturnsAsync(mockDeck);

//         var result = await service.RegisterUser(username, email, password);

//         result.Should().NotBeNull();
//         result.Username.Should().Be(username);
//         result.Email.Should().Be(email);
//         result.PasswordHash.Should().Be(password);
//         result.Deck.Should().NotBeNull();

//         deckServiceMock.Verify(x => x.InitializeDeck(It.IsAny<Guid>()), Times.Once);

//         var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
//         savedUser.Should().NotBeNull();
//     }

//     [Fact]
//     public async Task RegisterUser_WithExistingUsername_ShouldThrowUsernameExistsException()
//     {
//         using var context = _fixture.CreateContext();
//         var deckServiceMock = new Mock<IDeckService>();
//         var service = new UserService(context, deckServiceMock.Object);

//         var existingUser = new User
//         {
//             Id = Guid.NewGuid(),
//             Username = "existinguser",
//             Email = "existing@example.com",
//             PasswordHash = "password123"
//         };

//         context.Users.Add(existingUser);
//         await context.SaveChangesAsync();

//         var act = async () => await service.RegisterUser("existinguser", "new@example.com", "password123");

//         await act.Should().ThrowAsync<UsernameExistsException>();
//     }

//     [Fact]
//     public async Task RegisterUser_WithExistingEmail_ShouldThrowEmailExistsException()
//     {
//         using var context = _fixture.CreateContext();
//         var deckServiceMock = new Mock<IDeckService>();
//         var service = new UserService(context, deckServiceMock.Object);

//         var existingUser = new User
//         {
//             Id = Guid.NewGuid(),
//             Username = "existinguser",
//             Email = "existing@example.com",
//             PasswordHash = "password123"
//         };

//         context.Users.Add(existingUser);
//         await context.SaveChangesAsync();

//         var act = async () => await service.RegisterUser("newuser", "existing@example.com", "password123");

//         await act.Should().ThrowAsync<EmailExistsException>();
//     }

//     #endregion
// }
