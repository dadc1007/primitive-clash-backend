using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record JoinedToGameNotification(
    Guid GameId,
    GameState State,
    List<PlayerStateNotification> Players,
    ArenaNotification Arena
);
