using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class RefreshHandNotificationMapper
{
    public static RefreshHandNotification ToRefreshHandNotification(PlayerCard cardToPut, PlayerCard nextCard, decimal elixir)
    {
        PlayerCardNotification cardToPutNotification = new PlayerCardNotification(cardToPut.UserId, cardToPut.Id,
            cardToPut.Card.Id, cardToPut.Card.ElixirCost, cardToPut.Card.ImageUrl);        
        PlayerCardNotification nextCardNotification = new PlayerCardNotification(nextCard.UserId, nextCard.Id,
            nextCard.Card.Id, nextCard.Card.ElixirCost, nextCard.Card.ImageUrl);
        
        return new RefreshHandNotification(cardToPutNotification, nextCardNotification, elixir);
    }
}