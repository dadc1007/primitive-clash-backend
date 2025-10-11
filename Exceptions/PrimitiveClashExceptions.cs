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

    public class DeckNotFoundException : GameException
    {
        public DeckNotFoundException() : base("Deck not found for the user") { }
    }
}