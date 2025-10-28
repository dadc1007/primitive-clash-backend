namespace PrimitiveClash.Backend.DTOs.Notifications;

public record PlayerCardNotification(Guid PlayerId, Guid PlayerCardId, Guid CardId, int Elixir);