namespace PrimitiveClash.Backend.DTOs.Notifications;

public record RefreshHandNotification(
    PlayerCardNotification CardToPut,
    PlayerCardNotification NextCard,
    decimal Elixir
);
