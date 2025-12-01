using FluentAssertions;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Exceptions;

public class PrimitiveClashExceptionsTests
{
    [Fact]
    public void CardNotFoundException_HasCorrectMessage()
    {
        // Act
        var exception = new CardNotFoundException();

        // Assert
        exception.Should().BeOfType<CardNotFoundException>();
        exception.Message.Should().Be("Card not found");
    }

    [Fact]
    public void CardNotInHandException_HasCorrectMessage()
    {
        // Act
        var exception = new CardNotInHandException();

        // Assert
        exception.Should().BeOfType<CardNotInHandException>();
        exception.Message.Should().Be("Card not in hand");
    }

    [Fact]
    public void CardsMissingException_HasCorrectMessage()
    {
        // Act
        var exception = new CardsMissingException();

        // Assert
        exception.Should().BeOfType<CardsMissingException>();
        exception.Message.Should().Be("Not all required initial cards templates were found in the database");
    }

    [Fact]
    public void ConcurrencyException_HasCorrectMessageWithParameters()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var maxRetries = 5;

        // Act
        var exception = new ConcurrencyException(sessionId, maxRetries);

        // Assert
        exception.Should().BeOfType<ConcurrencyException>();
        exception.Message.Should().Be($"Failed to update game state for session {sessionId} after {maxRetries} retries");
    }

    [Fact]
    public void EmailExistsException_HasCorrectMessageWithEmail()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var exception = new EmailExistsException(email);

        // Assert
        exception.Should().BeOfType<EmailExistsException>();
        exception.Message.Should().Be($"The email '{email}' is already taken");
    }

    [Fact]
    public void EnemyTowersNotFoundException_HasCorrectMessage()
    {
        // Act
        var exception = new EnemyTowersNotFoundException();

        // Assert
        exception.Should().BeOfType<EnemyTowersNotFoundException>();
        exception.Message.Should().Be("Enemy towers not found");
    }

    [Fact]
    public void GameException_CanBeCreatedWithCustomMessage()
    {
        // Arrange
        var customMessage = "Custom game error";

        // Act
        var exception = new GameException(customMessage);

        // Assert
        exception.Should().BeOfType<GameException>();
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void NotEnoughPlayerCardsException_HasCorrectMessage()
    {
        // Act
        var exception = new NotEnoughPlayerCardsException();

        // Assert
        exception.Message.Should().Be("Not enough player cards to upgrade");
    }

    [Fact]
    public void CardAlreadyInDeckException_HasCorrectMessage()
    {
        // Act
        var exception = new CardAlreadyInDeckException();

        // Assert
        exception.Message.Should().Be("The card is already in the deck");
    }

    [Fact]
    public void CardNotInDeckException_HasCorrectMessage()
    {
        // Act
        var exception = new CardNotInDeckException();

        // Assert
        exception.Message.Should().Be("The card is not in the deck");
    }

    [Fact]
    public void NotEnoughGoldException_HasCorrectMessage()
    {
        // Act
        var exception = new NotEnoughGoldException();

        // Assert
        exception.Message.Should().Be("Not enough gold");
    }

    [Fact]
    public void NotEnoughGemsException_HasCorrectMessage()
    {
        // Act
        var exception = new NotEnoughGemsException();

        // Assert
        exception.Message.Should().Be("Not enough gems");
    }

    [Fact]
    public void NotEnoughTrophiesException_HasCorrectMessage()
    {
        // Act
        var exception = new NotEnoughTrophiesException();

        // Assert
        exception.Message.Should().Be("The number of trophies lost cannot be greater than the current amount");
    }

    [Fact]
    public void InvalidDeckSizeException_HasCorrectMessageWithNumber()
    {
        // Arrange
        var deckSize = 8;

        // Act
        var exception = new InvalidDeckSizeException(deckSize);

        // Assert
        exception.Message.Should().Be($"A deck can only have {deckSize} cards");
    }

    [Fact]
    public void DeckNotFoundException_HasCorrectMessageWithUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var exception = new DeckNotFoundException(userId);

        // Assert
        exception.Message.Should().Be($"Deck not found for the user with ID '{userId}'");
    }

    [Fact]
    public void InvalidCredentialsException_HasCorrectMessage()
    {
        // Act
        var exception = new InvalidCredentialsException();

        // Assert
        exception.Message.Should().Be("Invalid username or password");
    }

    [Fact]
    public void PlayerAlreadyInQueueException_HasCorrectMessageWithUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var exception = new PlayerAlreadyInQueueException(userId);

        // Assert
        exception.Message.Should().Be($"The player with ID '{userId}' is already in the matchmaking queue.");
    }

    [Fact]
    public void TowerTemplateNotFoundException_HasCorrectMessageWithType()
    {
        // Arrange
        var towerType = "Guardian";

        // Act
        var exception = new TowerTemplateNotFoundException(towerType);

        // Assert
        exception.Message.Should().Be($"The tower template of type '{towerType}' was not found");
    }

    [Fact]
    public void DefaultArenaTemplateNotFoundException_HasCorrectMessage()
    {
        // Act
        var exception = new DefaultArenaTemplateNotFoundException();

        // Assert
        exception.Message.Should().Be("There are no arena templates in the database");
    }

    [Fact]
    public void InvalidPlayersNumberException_HasCorrectMessage()
    {
        // Act
        var exception = new InvalidPlayersNumberException();

        // Assert
        exception.Message.Should().Be("There must be two players per game");
    }

    [Fact]
    public void GameNotFoundException_HasCorrectMessageWithId()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        // Act
        var exception = new GameNotFoundException(gameId);

        // Assert
        exception.Message.Should().Be($"The game with ID '{gameId}' was not found");
    }

    [Fact]
    public void InvalidGameDataException_HasCorrectMessageWithId()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        // Act
        var exception = new InvalidGameDataException(gameId);

        // Assert
        exception.Message.Should().Be($"Failed to deserialize game data for ID {gameId}");
    }

    [Fact]
    public void InvalidSpawnPositionException_HasCorrectMessageWithCoordinates()
    {
        // Arrange
        var x = 10;
        var y = 15;

        // Act
        var exception = new InvalidSpawnPositionException(x, y);

        // Assert
        exception.Message.Should().Be($"Cannot spawn unit at position ({x}, {y}). Cell not walkable or occupied.");
    }

    [Fact]
    public void NotEnoughElixirException_HasCorrectMessageWithValues()
    {
        // Arrange
        float required = 5.0f;
        decimal available = 3.5m;

        // Act
        var exception = new NotEnoughElixirException(required, available);

        // Assert
        exception.Message.Should().Be($"Not enough elixir. Required: {required}, available: {available}.");
    }

    [Fact]
    public void InvalidCardException_HasCorrectMessageWithCardId()
    {
        // Arrange
        var cardId = Guid.NewGuid();

        // Act
        var exception = new InvalidCardException(cardId);

        // Assert
        exception.Message.Should().Be($"Card with ID '{cardId}' is invalid or not in player's hand.");
    }

    [Fact]
    public void PlayerNotInGameException_HasCorrectMessageWithUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var exception = new PlayerNotInGameException(userId);

        // Assert
        exception.Message.Should().Be($"Player with ID '{userId}' is not in the game.");
    }

    [Fact]
    public void CardTypeException_HasCorrectMessageWithCardType()
    {
        // Arrange
        var cardType = CardType.Building;

        // Act
        var exception = new CardTypeException(cardType);

        // Assert
        exception.Message.Should().Be($"Unsupported card type: {cardType}");
    }

    [Fact]
    public void InvalidArenaSideException_HasCorrectMessage()
    {
        // Act
        var exception = new InvalidArenaSideException();

        // Assert
        exception.Message.Should().Be("A card can be placed only in the player side arena");
    }

    [Fact]
    public void UsernameExistsException_HasCorrectMessageWithUsername()
    {
        // Arrange
        var username = "existinguser";

        // Act
        var exception = new UsernameExistsException(username);

        // Assert
        exception.Message.Should().Be($"The username '{username}' is already taken");
    }
}
