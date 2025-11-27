using PrimitiveClash.Backend.DTOs.Card.Responses;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class CardMapper
{
    public static CardResponse ToCardResponse(this Card card)
    {
        return card switch
        {
            TroopCard t => ToTroopResponse(t),
            _ => throw new InvalidOperationException(),
        };
    }

    private static CardResponse ToTroopResponse(TroopCard t)
    {
        return new CardResponse(
            t.Id,
            t.Name,
            t.ElixirCost,
            t.Rarity,
            t.Type,
            t.Damage,
            t.Targets,
            new AttackDetails(t.Hp, t.Range, t.UnitClass),
            new TroopDetails(t.VisionRange)
        );
    }
}
