using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record PlayerStateNotification(
    Guid Id,
    string Name,
    ArenaPosition ArenaPosition,
    bool IsConnected,
    string? ConnectionId,
    decimal CurrentElixir
);
