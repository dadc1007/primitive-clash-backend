using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Exceptions
{
    public class GameException(string message) : Exception(message) { }

    public class NotEnoughPlayerCardsException() : GameException("Not enough player cards to upgrade");

    public class CardAlreadyInDeckException() : GameException("The card is already in the deck");

    public class CardNotInDeckException() : GameException("The card is not in the deck");

    public class NotEnoughGoldException() : GameException("Not enough gold");

    public class NotEnoughGemsException() : GameException("Not enough gems");

    public class NotEnoughTrophiesException()
        : GameException("The number of trophies lost cannot be greater than the current amount");

    public class CardsMissingException()
        : GameException("Not all required initial cards templates were found in the database");

    public class InvalidDeckSizeException(int number) : GameException($"A deck can only have {number} cards") { }

    public class DeckNotFoundException(Guid userId) : GameException($"Deck not found for the user with ID '{userId}'") { }

    public class UsernameExistsException(string username) : GameException($"The username '{username}' is already taken") { }

    public class EmailExistsException(string email) : GameException($"The email '{email}' is already taken") { }

    public class InvalidCredentialsException() : GameException("Invalid username or password");

    public class PlayerAlreadyInQueueException(Guid userId) : GameException($"The player with ID '{userId}' is already in the matchmaking queue.") { }

    public class TowerTemplateNotFoundException(string type) : GameException($"The tower template of type '{type}' was not found") { }

    public class DefaultArenaTemplateNotFoundException() : GameException("There are no arena templates in the database");

    public class InvalidPlayersNumberException() : GameException("There must be two players per game");

    public class GameNotFoundException(Guid id) : GameException($"The game with ID '{id}' was not found") { }

    public class InvalidGameDataException(Guid id) : GameException($"Failed to deserialize game data for ID {id}") { }

    public class ConcurrencyException(Guid sessionId, int maxRetries) : GameException($"Failed to update game state for session {sessionId} after {maxRetries} retries") { }

    public class InvalidSpawnPositionException(int x, int y) : GameException($"Cannot spawn unit at position ({x}, {y}). Cell not walkable or occupied.") { }

    public class NotEnoughElixirException(float required, decimal available) : GameException($"Not enough elixir. Required: {required}, available: {available}.") { }

    public class InvalidCardException(Guid cardId) : GameException($"Card with ID '{cardId}' is invalid or not in player's hand.") { }

    public class PlayerNotInGameException(Guid userId) : GameException($"Player with ID '{userId}' is not in the game.") { }

    public class CardTypeException(CardType type) : GameException($"Unsupported card type: {type}") { }

    public class EnemyTowersNotFoundException() : GameException("Enemy towers not found");
    
    public class CardNotInHandException() : GameException("Card not in hand");
    
    public class InvalidArenaSideException() : GameException("A card can be placed only in the player side arena");
    
    public class CardNotFoundException() : GameException("Card not found");
}