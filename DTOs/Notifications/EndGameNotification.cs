namespace PrimitiveClash.Backend.DTOs.Notifications;

public record EndGameNotification(Guid WinnerId, Guid LosserId, int TowersWinner, int TowersLosser);