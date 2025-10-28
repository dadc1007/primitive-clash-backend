namespace PrimitiveClash.Backend.DTOs.Notifications;

public record CardSpawnedNotification(Guid UnitId, Guid UserId, Guid CardId, int Level, int X, int Y);