namespace PrimitiveClash.Backend.Exceptions
{
    public class GameException(string message) : Exception(message) { }

    public class NotEnoughPlayerCardsException : GameException
    {
        public NotEnoughPlayerCardsException() : base("Not enough player cards to upgrade") { }
    }

    public class CardAlreadyInDeckException : GameException
    {
        public CardAlreadyInDeckException() : base("The card is already in the deck") { }
    }

    public class CardNotInDeckException : GameException
    {
        public CardNotInDeckException() : base("The card is not in the deck") { }
    }

    public class NotEnoughGoldException : GameException
    {
        public NotEnoughGoldException() : base("Not enough gold") { }
    }

    public class NotEnoughGemsException : GameException
    {
        public NotEnoughGemsException() : base("Not enough gems") { }
    }

    public class NotEnoughTrophiesException : GameException
    {
        public NotEnoughTrophiesException() : base("The number of trophies lost cannot be greater than the current amount") { }
    }

    public class CardsMissingException : GameException
    {
        public CardsMissingException() : base("Not all required initial cards templates were found in the database") { }
    }

    public class InvalidDeckSizeException(int number) : GameException($"A deck can only have {number} cards") { }

    public class DeckNotFoundException(Guid userId) : GameException($"Deck not found for the user with ID '{userId}'") { }

    public class UsernameExistsException(string username) : GameException($"The username '{username}' is already taken") { }

    public class EmailExistsException(string email) : GameException($"The email '{email}' is already taken") { }

    public class InvalidCredentialsException : GameException
    {
        public InvalidCredentialsException() : base("Invalid username or password") { }
    }

    public class PlayerAlreadyInQueueException(Guid userId) : GameException($"The player with ID '{userId}' is already in the matchmaking queue.") { }

    public class TowerTemplateNotFoundException(string type) : GameException($"The tower template of type '{type}' was not found") { }

    public class DefaultArenaTemplateNotFoundException : GameException
    {
        public DefaultArenaTemplateNotFoundException() : base("There are no arena templates in the database") { }
    }

    public class InvalidPlayersNumberException : GameException
    {
        public InvalidPlayersNumberException() : base("There must be two players per game") { }
    }

    public class GameNotFoundException(Guid id) : GameException($"The game with ID '{id}' was not found") { }

    public class InvalidGameDataException(Guid id) : GameException($"Failed to deserialize game data for ID {id}") { }

    public class ConcurrencyException(Guid sessionId, int maxRetries) : GameException($"Failed to update game state for session {sessionId} after {maxRetries} retries") { }
}