// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using PrimitiveClash.Backend.Data;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Tests.Infrastructure;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Data;

// public class DbSeederTests : IClassFixture<DatabaseFixture>
// {
//     private readonly DatabaseFixture _fixture;

//     public DbSeederTests(DatabaseFixture fixture)
//     {
//         _fixture = fixture;
//     }

//     #region Seed Tests

//     [Fact]
//     public void Seed_WithEmptyDatabase_ShouldAddCardsAndArenaTemplate()
//     {
//         using var context = _fixture.CreateContext();

//         DbSeeder.Seed(context);

//         context.Cards.Should().NotBeEmpty();
//         context.Cards.Count().Should().Be(8);
//         context.ArenaTemplates.Should().NotBeEmpty();
//         context.ArenaTemplates.Count().Should().Be(1);

//         var arena = context.ArenaTemplates.First();
//         arena.Name.Should().Be("Valle Primitivo");
//         arena.RequiredTrophies.Should().Be(0);
//     }

//     [Fact]
//     public void Seed_WithExistingData_ShouldNotDuplicateData()
//     {
//         using var context = _fixture.CreateContext();

//         DbSeeder.Seed(context);
//         var initialCardCount = context.Cards.Count();
//         var initialArenaCount = context.ArenaTemplates.Count();

//         DbSeeder.Seed(context);

//         context.Cards.Count().Should().Be(initialCardCount);
//         context.ArenaTemplates.Count().Should().Be(initialArenaCount);
//     }

//     #endregion
// }
