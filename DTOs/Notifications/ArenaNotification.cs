using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.DTOs.Notifications;

public record ArenaNotification(
    Guid Id,
    ArenaTemplate ArenaTemplate,
    List<TowerNotification> Towers,
    List<CardSpawnedNotification> Entities
);
