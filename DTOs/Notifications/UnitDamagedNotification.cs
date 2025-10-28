namespace PrimitiveClash.Backend.DTOs.Notifications;

public record UnitDamagedNotification(
    Guid AttackerId,
    Guid TargetId,
    int Damage,
    int Health
);
