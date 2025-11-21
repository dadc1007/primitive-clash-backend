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
                        Id = new Guid("6ce2da92-188d-4458-b9a3-8f4e7ae02864"),
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
                        Id = new Guid("1a4b313f-5dff-4163-8dc5-eb2bf2df0a41"),
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
                        Id = new Guid("8b76c8c0-614f-4783-84f5-a9965eb01093"),
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
                        Id = new Guid("fe53ae3f-9575-48e0-847e-ad675cb51e7b"),
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
                        Id = new Guid("14a6debd-dc5a-42c6-a46a-ba2f137c6a50"),
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
                        Id = new Guid("29a0c77e-54da-40ec-afbb-cdd5449fd40f"),
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
                        Id = new Guid("0a1ae662-f5a3-4826-ab65-42d37b997154"),
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
                        Id = new Guid("6b75ea3b-a3b5-4ec6-8a38-31ca352bee55"),
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
                    Id = new Guid(),
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
                        Id = new Guid("4e718199-d25b-4f0f-88e2-221a43eb5dc6"),
                        Hp = 3200,
                        Damage = 120,
                        Range = 6,
                        Type = TowerType.Leader,
                        Size = 4,
                    },
                    new TowerTemplate
                    {
                        Id = new Guid("dc719076-4eea-4ec8-9d49-732d440cb27f"),
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
