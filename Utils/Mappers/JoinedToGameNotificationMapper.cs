using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class JoinedToGameNotificationMapper
{
    public static JoinedToGameNotification ToJoinedToGameNotification(Game game)
    {
        return new JoinedToGameNotification(
            game.Id,
            game.State,
            ToPlayerStateNotifications(game.PlayerStates),
            ToArenaNotification(game.GameArena)
        );
    }

    private static List<PlayerStateNotification> ToPlayerStateNotifications(
        List<PlayerState> playerStates
    )
    {
        List<PlayerStateNotification> playerStateNotifications = [];
        playerStateNotifications.AddRange(
            playerStates.Select(playerState => new PlayerStateNotification(
                playerState.Id,
                playerState.Username,
                playerState.ArenaPosition,
                playerState.IsConnected,
                playerState.ConnectionId,
                playerState.CurrentElixir
            ))
        );
        return playerStateNotifications;
    }

    private static ArenaNotification ToArenaNotification(Arena arena)
    {
        return new ArenaNotification(
            arena.Id,
            arena.ArenaTemplate,
            ToTowerNotification(arena.Towers),
            ToCardSpawnedNotification(arena.Entities)
        );
    }

    private static List<TowerNotification> ToTowerNotification(Dictionary<Guid, List<Tower>> towers)
    {
        return (
            from kvp in towers
            from tower in kvp.Value
            select new TowerNotification(
                tower.Id,
                tower.TowerTemplate.Id,
                tower.UserId,
                tower.TowerTemplate.Type,
                tower.Health,
                tower.TowerTemplate.Hp,
                tower.X,
                tower.Y
            )
        ).ToList();
    }

    private static List<CardSpawnedNotification> ToCardSpawnedNotification(
        Dictionary<Guid, List<ArenaEntity>> entities
    )
    {
        return (
            from kvp in entities
            from entity in kvp.Value
            select new CardSpawnedNotification(
                entity.Id,
                entity.UserId,
                entity.PlayerCard.Card.Id,
                entity.PlayerCard.Level,
                entity.X,
                entity.Y,
                entity.Health,
                (entity.PlayerCard.Card as AttackCard)!.Hp
            )
        ).ToList();
    }
}
