// using FluentAssertions;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Models.Cards;
// using PrimitiveClash.Backend.Models.Enums;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Models;

// public class PlayerCardTests
// {
//     private static PlayerCard CreateTestPlayerCard()
//     {
//         return new PlayerCard
//         {
//             Id = Guid.NewGuid(),
//             CardId = Guid.NewGuid(),
//             UserId = Guid.NewGuid(),
//             Level = 1,
//             Card = new TroopCard
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "TestCard",
//                 ElixirCost = 3,
//                 Rarity = CardRarity.Common,
//                 Type = CardType.Troop,
//                 Damage = 100,
//                 Targets = [CardTarget.Ground],
//                 Hp = 300,
//                 Range = 1,
//                 HitSpeed = 1.0f,
//                 MovementSpeed = MovementSpeed.Medium
//             }
//         };
//     }

//     #region IncreaseQuantity Tests

//     [Fact]
//     public void IncreaseQuantity_WithPositiveAmount_ShouldIncreaseQuantity()
//     {
//         // Arrange
//         var playerCard = CreateTestPlayerCard();
//         var initialQuantity = playerCard.Quantity;

//         // Act
//         playerCard.IncreaseQuantity(10);

//         // Assert
//         playerCard.Quantity.Should().Be(initialQuantity + 10);
//     }

//     #endregion

//     #region Upgrade Tests

//     [Fact]
//     public void Upgrade_WithSufficientQuantity_ShouldIncreaseLevelAndDecreaseQuantity()
//     {
//         var playerCard = CreateTestPlayerCard();
//         playerCard.IncreaseQuantity(50);
//         var initialLevel = playerCard.Level;
//         var initialQuantity = playerCard.Quantity;

//         playerCard.Upgrade(20);

//         playerCard.Level.Should().Be(initialLevel + 1);
//         playerCard.Quantity.Should().Be(initialQuantity - 20);
//     }

//     [Fact]
//     public void Upgrade_WithInsufficientQuantity_ShouldThrowNotEnoughPlayerCardsException()
//     {
//         var playerCard = CreateTestPlayerCard();
//         playerCard.IncreaseQuantity(10);

//         var act = () => playerCard.Upgrade(20);

//         act.Should().Throw<NotEnoughPlayerCardsException>();
//     }

//     #endregion
// }
