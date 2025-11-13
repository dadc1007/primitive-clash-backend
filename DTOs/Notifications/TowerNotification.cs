using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record TowerNotification(
    Guid Id,
    Guid TowerTemplateId,
    Guid UserId,
    TowerType Type,
    int Health,
    int MaxHealth,
    int X,
    int Y
);
