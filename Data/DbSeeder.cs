using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Cards.Any())
            {
                List<Card> cards = [
                    new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Guerrero Tribal",
                   ElixirCost = 3,
                   Rarity = CardRarity.Common,
                   Type = CardType.Troop,
                   Damage = 90,
                   Targets = [CardTarget.Ground],
                   Hp = 350,
                   Range = 1,
                   HitSpeed = 1.2F,
                   MovementSpeed = MovementSpeed.Medium
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Cazadora Salvaje",
                   ElixirCost = 4,
                   Rarity = CardRarity.Rare,
                   Type = CardType.Troop,
                   Damage = 70,
                   Targets = [CardTarget.Ground, CardTarget.Air],
                   Hp = 250,
                   Range = 5,
                   HitSpeed = 1.1F,
                   MovementSpeed = MovementSpeed.Medium
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Jabalí Enfurecido",
                   ElixirCost = 4,
                   Rarity = CardRarity.Epic,
                   Type = CardType.Troop,
                   Damage = 120,
                   Targets = [CardTarget.Buildings],
                   Hp = 500,
                   Range = 1,
                   HitSpeed = 1.6F,
                   MovementSpeed = MovementSpeed.Fast
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Chamán del Fuego",
                   ElixirCost = 5,
                   Rarity = CardRarity.Epic,
                   Type = CardType.Troop,
                   Damage = 80,
                   Targets = [CardTarget.Ground, CardTarget.Air],
                   Hp = 400,
                   Range = 4,
                   DamageArea = 2,
                   HitSpeed = 0.8F,
                   MovementSpeed = MovementSpeed.Slow
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Mamut de Guerra",
                   ElixirCost = 6,
                   Rarity = CardRarity.Legendary,
                   Type = CardType.Troop,
                   Damage = 200,
                   Targets = [CardTarget.Ground],
                   Hp = 1200,
                   Range = 1,
                   DamageArea = 2,
                   HitSpeed = 1.8F,
                   MovementSpeed = MovementSpeed.Slow
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Lanzadora de Huesos",
                   ElixirCost = 3,
                   Rarity = CardRarity.Common,
                   Type = CardType.Troop,
                   Damage = 60,
                   Targets = [CardTarget.Ground],
                   Hp = 230,
                   Range = 4,
                   HitSpeed = 1.0F,
                   MovementSpeed = MovementSpeed.Medium
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Tigre Dientes de Sable",
                   ElixirCost = 5,
                   Rarity = CardRarity.Rare,
                   Type = CardType.Troop,
                   Damage = 150,
                   Targets = [CardTarget.Ground],
                   Hp = 600,
                   Range = 1,
                   DamageArea = 1,
                   HitSpeed = 1.3F,
                   MovementSpeed = MovementSpeed.Fast
               },
               new TroopCard
               {
                   Id = Guid.NewGuid(),
                   Name = "Guardián del Trueno",
                   ElixirCost = 7,
                   Rarity = CardRarity.Legendary,
                   Type = CardType.Troop,
                   Damage = 100,
                   Targets = [CardTarget.Ground, CardTarget.Air],
                   Hp = 800,
                   Range = 5,
                   DamageArea = 3,
                   HitSpeed = 1.5F,
                   MovementSpeed = MovementSpeed.Medium
               }
               ];

                context.Cards.AddRange(cards);
            }

            if (!context.ArenaTemplates.Any())
            {
                ArenaTemplate arena = new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Valle Primitivo",
                    RequiredTrophies = 0
                };

                context.ArenaTemplates.Add(arena);
            }

            context.SaveChanges();
        }
    }
}