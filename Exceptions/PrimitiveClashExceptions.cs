namespace PrimitiveClash.Backend.Exceptions
{
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
    }

    public class NotEnoughPlayerCardsException : GameException
    {
        public NotEnoughPlayerCardsException() : base("Not enough player cards to upgrade") { }
    }

    public class LimitCardsInDeckException : GameException
    {
        public LimitCardsInDeckException() : base("A deck can only have 8 cards") { }
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
}