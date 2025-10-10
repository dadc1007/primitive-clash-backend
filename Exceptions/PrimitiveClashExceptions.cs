namespace PrimitiveClash.Backend.Exceptions
{
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
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
}