using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class UnitDamagedNotificationMapper
{
    public static UnitDamagedNotification ToUnitDamagedNotification(Positioned attacker, Positioned target, int damge)
    {
        int maxHealth = target switch
        {
            ArenaEntity arenaEntity => (arenaEntity.PlayerCard.Card as AttackCard)!.Hp,
            Tower tower => tower.TowerTemplate.Hp,
            _ => 0
        };

        return new UnitDamagedNotification(attacker.Id, target.Id, damge, target.Health, maxHealth);
    }
}