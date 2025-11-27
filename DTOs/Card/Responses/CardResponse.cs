using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Card.Responses;

public record CardResponse(
    Guid Id,
    string Name,
    int ElixirCost,
    CardRarity Rarity,
    CardType Type,
    int Damage,
    List<UnitClass> Targets,
    AttackDetails? AttackDetails,
    TroopDetails? TroopDetails
);

public record AttackDetails(int Hp, int Range, UnitClass UnitClass);
public record TroopDetails(int VisionRange);
