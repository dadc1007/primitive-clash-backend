using PrimitiveClash.Backend.Exceptions;

namespace PrimitiveClash.Backend.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public int Gold { get; private set; } = 1000;
        public int Gems { get; private set; } = 100;
        public int Level { get; private set; } = 1;
        public int Trophies { get; private set; } = 0;
        public List<PlayerCard> PlayerCards { get; set; } = [];
        public Deck? Deck { get; set; }
        public Guid? MatchId { get; set; }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public void SpendGold(int amount)
        {
            if (amount > Gold)
                throw new NotEnoughGoldException();
            Gold -= amount;
        }

        public void AddGems(int amount)
        {
            Gems += amount;
        }

        public void SpendGems(int amount)
        {
            if (amount > Gems)
                throw new NotEnoughGemsException();
            Gems -= amount;
        }

        public void AddTrophies(int amount)
        {
            Trophies += amount;
        }

        public void RemoveTrophies(int amount)
        {
            if (amount > Trophies)
                throw new NotEnoughTrophiesException();
            Trophies -= amount;
        }

        public void LevelUp()
        {
            Level++;
        }
    }
}