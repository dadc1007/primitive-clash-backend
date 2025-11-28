namespace PrimitiveClash.Backend.DTOs.Notifications;

public record PlayerStateNotification(
    Guid Id,
    bool IsConnected,
    string? ConnectionId,
    decimal CurrentElixir
);
