namespace PrimitiveClash.Backend.DTOs.Notifications;

public record EndGameNotification(Guid winnerId, Guid losserId, int towersWinner, int towersLosser);