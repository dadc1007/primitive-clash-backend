using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class PlayerHandNotificationMapper
{
    public static PlayerHandNotification ToPlayerHandNotification(Arena arena, PlayerState player)
    {
        List<PlayerCardNotification> hand = [];
        PlayerCard nextCard = player.GetNextCard();
        hand.AddRange(
            player
                .GetHand()
                .Select(playerCard =>
                    (
                        new PlayerCardNotification(
                            player.Id,
                            playerCard.Id,
                            playerCard.CardId,
                            playerCard.Card.ElixirCost,
                            playerCard.Card.ImageUrl
                        )
                    )
                )
        );

        return new PlayerHandNotification(
            hand,
            new PlayerCardNotification(
                player.Id,
                nextCard.Id,
                nextCard.CardId,
                nextCard.Card.ElixirCost,
                nextCard.Card.ImageUrl
            )
        );
    }
}
