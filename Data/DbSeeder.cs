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
                List<Card> cards =
                [
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cavernicola",
                        ElixirCost = 2,
                        Rarity = CardRarity.Common,
                        Type = CardType.Troop,
                        Damage = 80,
                        Targets = [UnitClass.Ground],
                        Hp = 320,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.1F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Fast,
                        VisionRange = 5,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Caveman.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cavernicola de Lanza",
                        ElixirCost = 3,
                        Rarity = CardRarity.Common,
                        Type = CardType.Troop,
                        Damage = 70,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 280,
                        Range = 5,
                        DamageArea = 1,
                        HitSpeed = 1.0F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 6,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Lancer%20caveman.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Mini Dino",
                        ElixirCost = 3,
                        Rarity = CardRarity.Rare,
                        Type = CardType.Troop,
                        Damage = 110,
                        Targets = [UnitClass.Ground],
                        Hp = 450,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.3F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 5,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Mini%20dino.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Mini Dragon",
                        ElixirCost = 4,
                        Rarity = CardRarity.Rare,
                        Type = CardType.Troop,
                        Damage = 100,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 550,
                        Range = 4,
                        DamageArea = 2,
                        HitSpeed = 1.2F,
                        UnitClass = UnitClass.Air,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 6,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Mini%20dragon.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Dragon Prehistorico",
                        ElixirCost = 5,
                        Rarity = CardRarity.Epic,
                        Type = CardType.Troop,
                        Damage = 160,
                        Targets = [UnitClass.Ground, UnitClass.Air],
                        Hp = 950,
                        Range = 5,
                        DamageArea = 3,
                        HitSpeed = 1.6F,
                        UnitClass = UnitClass.Air,
                        MovementSpeed = MovementSpeed.Slow,
                        VisionRange = 7,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Prehistoric%20dragon.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pterodactilo",
                        ElixirCost = 3,
                        Rarity = CardRarity.Rare,
                        Type = CardType.Troop,
                        Damage = 90,
                        Targets = [UnitClass.Air, UnitClass.Ground],
                        Hp = 380,
                        Range = 3,
                        DamageArea = 1,
                        HitSpeed = 0.9F,
                        UnitClass = UnitClass.Air,
                        MovementSpeed = MovementSpeed.Fast,
                        VisionRange = 6,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Pterdactyl.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Golem de Piedra",
                        ElixirCost = 6,
                        Rarity = CardRarity.Epic,
                        Type = CardType.Troop,
                        Damage = 190,
                        Targets = [UnitClass.Buildings],
                        Hp = 1600,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.8F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Slow,
                        VisionRange = 5,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Rock%20golem.png",
                    },
                    new TroopCard
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cavernicola Guerrero",
                        ElixirCost = 4,
                        Rarity = CardRarity.Legendary,
                        Type = CardType.Troop,
                        Damage = 140,
                        Targets = [UnitClass.Ground],
                        Hp = 750,
                        Range = 1,
                        DamageArea = 1,
                        HitSpeed = 1.0F,
                        UnitClass = UnitClass.Ground,
                        MovementSpeed = MovementSpeed.Medium,
                        VisionRange = 6,
                        ImageUrl =
                            "https://gptxxmmpguxotekpzudp.supabase.co/storage/v1/object/public/primitve-clash-cards/Warrior%20caveman.png",
                    },
                ];

                context.Cards.AddRange(cards);
            }

            if (!context.ArenaTemplates.Any())
            {
                ArenaTemplate arena = new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Valle Primitivo",
                    RequiredTrophies = 0,
                };

                context.ArenaTemplates.Add(arena);
            }

            if (!context.TowerTemplates.Any())
            {
                List<TowerTemplate> towers =
                [
                    new TowerTemplate
                    {
                        Id = Guid.NewGuid(),
                        Hp = 3200,
                        Damage = 120,
                        Range = 6,
                        Type = TowerType.Leader,
                        Size = 4,
                    },
                    new TowerTemplate
                    {
                        Id = Guid.NewGuid(),
                        Hp = 2000,
                        Damage = 120,
                        Range = 5,
                        Type = TowerType.Guardian,
                        Size = 3,
                    },
                ];

                context.TowerTemplates.AddRange(towers);
            }

            context.SaveChanges();
        }
    }
}
