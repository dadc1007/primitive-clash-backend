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
                        Targets = [UnitClass.Ground],
                        Hp = 350,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.2F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 5
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cazador de Bestias",
                        ElixirCost = 4,
                        Rarity = CardRarity.Rare,
                        Type = CardType.Troop,
                        Damage = 130,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 420,
                        Range = 4,
                        DamageArea = 1,
                        HitSpeed = 1.4F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 6
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Lanzadora Tribal",
                        ElixirCost = 2,
                        Rarity = CardRarity.Common,
                        Type = CardType.Troop,
                        Damage = 65,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 250,
                        Range = 5,
                        DamageArea = 1,
                        HitSpeed = 1.0F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Fast,
                        VisionRange = 6
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Chaman Ancestral",
                        ElixirCost = 5,
                        Rarity = CardRarity.Epic,
                        Type = CardType.Troop,
                        Damage = 60,
                        Targets = [UnitClass.Ground],
                        Hp = 550,
                        Range = 5,
                        DamageArea = 3,
                        HitSpeed = 1.6F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Slow,
                        VisionRange = 6
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Jinete de Mamut",
                        ElixirCost = 6,
                        Rarity = CardRarity.Epic,
                        Type = CardType.Troop,
                        Damage = 200,
                        Targets = [UnitClass.Buildings],
                        Hp = 1000,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.8F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 5
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Águila Primordial",
                        ElixirCost = 4,
                        Rarity = CardRarity.Rare,
                        Type = CardType.Troop,
                        Damage = 110,
                        Targets = [UnitClass.Ground],
                        Hp = 380,
                        Range = 3,
                        DamageArea = 1,
                        HitSpeed = 1.2F,
                        UnitClass = UnitClass.Air,
                        MovementSpeed = MovementSpeed.Fast,
                        VisionRange = 7
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Titán de Piedra",
                        ElixirCost = 7,
                        Rarity = CardRarity.Legendary,
                        Type = CardType.Troop,
                        Damage = 280,
                        Targets = [UnitClass.Ground, UnitClass.Buildings],
                        Hp = 1600,
                        Range = 1,
                        DamageArea = 2,
                        HitSpeed = 2.2F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Slow,
                        VisionRange = 6
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Dragón Ancestral",
                        ElixirCost = 8,
                        Rarity = CardRarity.Legendary,
                        Type = CardType.Troop,
                        Damage = 180,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 1200,
                        Range = 4,
                        DamageArea = 2,
                        HitSpeed = 1.5F,
                        UnitClass = UnitClass.Air,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 7
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

            if (!context.TowerTemplates.Any())
            {
                List<TowerTemplate> towers = [
                    new TowerTemplate
                    {
                        Id = Guid.NewGuid(),
                        Hp = 3200,
                        Damage = 120,
                        Range = 6,
                        Type = TowerType.Leader,
                        Size = 4
                    },
                    new TowerTemplate
                    {
                        Id = Guid.NewGuid(),
                        Hp = 2000,
                        Damage = 120,
                        Range = 5,
                        Type = TowerType.Guardian,
                        Size = 4
                    }
                ];

                context.TowerTemplates.AddRange(towers);
            }

            context.SaveChanges();
        }
    }
}