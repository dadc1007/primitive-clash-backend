// using FluentAssertions;
// using Moq;
// using PrimitiveClash.Backend.Data;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Services;
// using PrimitiveClash.Backend.Services.Impl;
// using PrimitiveClash.Backend.Tests.Infrastructure;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Services;

// public class AuthServiceTests : IClassFixture<DatabaseFixture>
// {
//     private readonly DatabaseFixture _fixture;

//     public AuthServiceTests(DatabaseFixture fixture)
//     {
//         _fixture = fixture;
//     }

//     #region RegisterUser Tests

//     [Fact]
//     public async Task RegisterUser_WithValidData_ShouldCallUserServiceAndReturnUser()
//     {
//         using var context = _fixture.CreateContext();
//         var userServiceMock = new Mock<IUserService>();
//         var service = new AuthService(context, userServiceMock.Object);

//         var username = "testuser";
//         var email = "test@example.com";
//         var password = "password123";
//         var expectedUser = new User
//         {
//             Id = Guid.NewGuid(),
//             Username = username,
//             Email = email,
//             PasswordHash = password
//         };

//         userServiceMock
//             .Setup(x => x.RegisterUser(username, email, password))
//             .ReturnsAsync(expectedUser);

//         var result = await service.RegisterUser(username, email, password);

//         result.Should().NotBeNull();
//         result.Username.Should().Be(username);
//         result.Email.Should().Be(email);
//         userServiceMock.Verify(x => x.RegisterUser(username, email, password), Times.Once);
//     }

//     [Fact]
//     public async Task RegisterUser_WithExistingUsername_ShouldThrowUsernameExistsException()
//     {
//         using var context = _fixture.CreateContext();
//         var userServiceMock = new Mock<IUserService>();
//         var service = new AuthService(context, userServiceMock.Object);

//         var username = "existinguser";
//         var email = "new@example.com";
//         var password = "password123";

//         userServiceMock
//             .Setup(x => x.RegisterUser(username, email, password))
//             .ThrowsAsync(new UsernameExistsException(username));

//         var act = async () => await service.RegisterUser(username, email, password);

//         await act.Should().ThrowAsync<UsernameExistsException>();
//     }

//     #endregion

//     #region LoginUser Tests

//     [Fact]
//     public async Task LoginUser_WithValidCredentials_ShouldReturnUser()
//     {
//         using var context = _fixture.CreateContext();
//         var userServiceMock = new Mock<IUserService>();
//         var service = new AuthService(context, userServiceMock.Object);

//         var user = new User
//         {
//             Id = Guid.NewGuid(),
//             Username = "testuser",
//             Email = "test@example.com",
//             PasswordHash = "password123"
//         };

//         context.Users.Add(user);
//         await context.SaveChangesAsync();

//         var result = await service.LoginUser(user.Email, user.PasswordHash);

//         result.Should().NotBeNull();
//         result.Email.Should().Be(user.Email);
//         result.Username.Should().Be(user.Username);
//     }

//     [Fact]
//     public async Task LoginUser_WithInvalidEmail_ShouldThrowInvalidCredentialsException()
//     {
//         using var context = _fixture.CreateContext();
//         var userServiceMock = new Mock<IUserService>();
//         var service = new AuthService(context, userServiceMock.Object);

//         var act = async () => await service.LoginUser("nonexistent@example.com", "password123");

//         await act.Should().ThrowAsync<InvalidCredentialsException>();
//     }

//     [Fact]
//     public async Task LoginUser_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
//     {
//         using var context = _fixture.CreateContext();
//         var userServiceMock = new Mock<IUserService>();
//         var service = new AuthService(context, userServiceMock.Object);

//         var user = new User
//         {
//             Id = Guid.NewGuid(),
//             Username = "testuser",
//             Email = "test@example.com",
//             PasswordHash = "correctpassword"
//         };

//         context.Users.Add(user);
//         await context.SaveChangesAsync();

//         var act = async () => await service.LoginUser(user.Email, "wrongpassword");

//         await act.Should().ThrowAsync<InvalidCredentialsException>();
//     }

//     #endregion
// }
