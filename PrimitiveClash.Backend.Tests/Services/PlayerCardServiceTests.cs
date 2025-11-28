// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using PrimitiveClash.Backend.Configuration;
// using PrimitiveClash.Backend.Data;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Models.Cards;
// using PrimitiveClash.Backend.Models.Enums;
// using PrimitiveClash.Backend.Services.Impl;
// using PrimitiveClash.Backend.Tests.Infrastructure;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Services;

// public class PlayerCardServiceTests : IClassFixture<DatabaseFixture>
// {
//     private readonly DatabaseFixture _fixture;
//     private readonly List<string> _starterCardNames = ["Card1", "Card2", "Card3"];

//     public PlayerCardServiceTests(DatabaseFixture fixture)
//     {
//         _fixture = fixture;
//     }

//     #region CreateStarterCards Tests

//     [Fact]
//     public async Task CreateStarterCards_WithAllCardsAvailable_ShouldCreatePlayerCards()
//     {
//         using var context = _fixture.CreateContext();
//         var gameSettings = new GameSettings 
//         { 
//             StarterCardNames = _starterCardNames,
//             MaxDeckSize = 8
//         };
//         var gameSettingsOptions = Options.Create(gameSettings);
//         var service = new PlayerCardService(context, gameSettingsOptions);

//         var userId = Guid.NewGuid();
//         var deckId = Guid.NewGuid();

//         var cards = _starterCardNames.Select(name => new TroopCard
//         {
//             Id = Guid.NewGuid(),
//             Name = name,
//             ElixirCost = 3,
//             Rarity = CardRarity.Common,
//             Type = CardType.Troop,
//             Damage = 100,
//             Targets = [CardTarget.Ground],
//             Hp = 300,
//             Range = 1,
//             HitSpeed = 1.0f,
//             MovementSpeed = MovementSpeed.Medium
//         }).ToList();

//         context.Cards.AddRange(cards);
//         await context.SaveChangesAsync();

//         var result = await service.CreateStarterCards(userId, deckId);

//         result.Should().NotBeNull();
//         result.Should().HaveCount(_starterCardNames.Count);
//         result.Should().AllSatisfy(pc =>
//         {
//             pc.UserId.Should().Be(userId);
//             pc.DeckId.Should().Be(deckId);
//             pc.Card.Should().NotBeNull();
//         });
//     }

//     [Fact]
//     public async Task CreateStarterCards_WithMissingCards_ShouldThrowCardsMissingException()
//     {
//         using var context = _fixture.CreateContext();
//         var gameSettings = new GameSettings 
//         { 
//             StarterCardNames = _starterCardNames,
//             MaxDeckSize = 8
//         };
//         var gameSettingsOptions = Options.Create(gameSettings);
//         var service = new PlayerCardService(context, gameSettingsOptions);

//         var userId = Guid.NewGuid();
//         var deckId = Guid.NewGuid();

//         var card = new TroopCard
//         {
//             Id = Guid.NewGuid(),
//             Name = "Card1",
//             ElixirCost = 3,
//             Rarity = CardRarity.Common,
//             Type = CardType.Troop,
//             Damage = 100,
//             Targets = [CardTarget.Ground],
//             Hp = 300,
//             Range = 1,
//             HitSpeed = 1.0f,
//             MovementSpeed = MovementSpeed.Medium
//         };

//         context.Cards.Add(card);
//         await context.SaveChangesAsync();

//         var act = async () => await service.CreateStarterCards(userId, deckId);

//         await act.Should().ThrowAsync<CardsMissingException>();
//     }

//     #endregion
// }
