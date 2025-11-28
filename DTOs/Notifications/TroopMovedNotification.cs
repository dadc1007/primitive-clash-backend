using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record TroopMovedNotification(
    Guid TroopId,
    Guid PlayerId,
    Guid CardId,
    int X,
    int Y,
    string State
);
