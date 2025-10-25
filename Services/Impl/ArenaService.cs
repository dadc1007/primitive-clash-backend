using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Factories;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaService(
        IArenaTemplateService arenaTemplateService, 
        IArenaEntityFactory arenaEntityFactory,
        ILogger<ArenaService> logger) : IArenaService
    {
        private readonly IArenaTemplateService _arenaTemplateService = arenaTemplateService;
        private readonly IArenaEntityFactory _arenaEntityFactory = arenaEntityFactory;
        private readonly ILogger<ArenaService> _logger = logger;

        public async Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate arenaTemplate = await _arenaTemplateService.GetDefaultArenaTemplate();

            return new Arena(arenaTemplate, towers);
        }

        public ArenaEntity PlaceEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y)
        {
            if (!arena.IsInsideBounds(x, y))
            {
                throw new InvalidSpawnPositionException(x, y);
            }

            ArenaEntity entity = _arenaEntityFactory.CreateEntity(player, card, x, y);
            arena.PlaceEntity(entity);

            return entity;
        }

        public double CalculateDistance(ArenaEntity entity1, ArenaEntity entity2)
        {
            return Math.Sqrt(Math.Pow(entity2.PosX - entity1.PosX, 2) + Math.Pow(entity2.PosY - entity1.PosY, 2));
        }

        public (AttackEntity? Target, double Distance) FindClosestTarget(Arena arena, TroopEntity troop, double visionRange)
        {
            _logger.LogDebug("Buscando objetivo para tropa {TroopId} del jugador {PlayerId}. Rango de visión: {VisionRange}", 
                troop.Id, troop.UserId, visionRange);

            // Obtener el ID del jugador enemigo
            Guid enemyUserId = arena.Towers.Keys.First(id => id != troop.UserId);
            _logger.LogDebug("ID del enemigo encontrado: {EnemyId}", enemyUserId);

            AttackEntity? closestThreat = null;
            double minDistance = double.MaxValue;

            // --- 1. PRIORIDAD ALTA: Buscar Tropas y Edificios del ENEMIGO en RANGO DE VISIÓN ---
            _logger.LogDebug("Buscando tropas enemigas. PlayerEntities contiene {Count} jugadores", arena.PlayerEntities.Count);
            
            if (arena.PlayerEntities.TryGetValue(enemyUserId, out var enemyEntities))
            {
                _logger.LogDebug("Encontradas {Count} entidades del enemigo", enemyEntities.Count);
                foreach (var entity in enemyEntities.OfType<AttackEntity>())
                {
                    // Solo necesitamos verificar que esté vivo y no sea una Torre
                    if (entity.IsAlive() && entity is not Tower)
                    {
                        double distance = CalculateDistance(troop, entity);
                        _logger.LogDebug("Entidad enemiga encontrada: {EntityId} (Tipo: {EntityType}) a distancia {Distance}", 
                            entity.Id, entity.GetType().Name, distance);

                        if (distance <= visionRange && distance < minDistance)
                        {
                            minDistance = distance;
                            closestThreat = entity;
                            _logger.LogDebug("Nueva amenaza más cercana encontrada: {EntityId} a distancia {Distance}", 
                                entity.Id, distance);
                        }
                    }
                }
            }
            else
            {
                _logger.LogDebug("No se encontraron entidades para el enemigo {EnemyId}", enemyUserId);
            }

            if (closestThreat is not null)
            {
                _logger.LogDebug("Retornando amenaza encontrada: {EntityId} a distancia {Distance}", 
                    closestThreat.Id, minDistance);
                return (closestThreat, minDistance);
            }

            // --- 2. PRIORIDAD BAJA: Buscar la Torre más cercana (Fallback global) ---
            _logger.LogDebug("Buscando torres enemigas como fallback");
            Tower? closestTower = null;
            minDistance = double.MaxValue; // Resetear la distancia mínima

            _logger.LogDebug("Verificando torres del jugador {PlayerId}. ¿Existe en arena.Towers? {HasTowers}", 
                enemyUserId, arena.Towers.ContainsKey(enemyUserId));

            if (!arena.Towers.ContainsKey(enemyUserId))
            {
                _logger.LogWarning("¡No se encontraron torres para el jugador {PlayerId}! Keys disponibles: {Keys}", 
                    enemyUserId, string.Join(", ", arena.Towers.Keys));
                return (null, 0);
            }

            var allEnemyTowers = arena.Towers[enemyUserId];
            _logger.LogDebug("Torres totales del enemigo: {Count}. Torres por tipo: Leader={LeaderCount}, Guardian={GuardianCount}",
                allEnemyTowers.Count,
                allEnemyTowers.Count(t => t.TowerTemplate.Type == TowerType.Leader),
                allEnemyTowers.Count(t => t.TowerTemplate.Type == TowerType.Guardian));

            foreach (var tower in allEnemyTowers)
            {
                _logger.LogDebug("Torre enemiga: ID={TowerId}, Tipo={Type}, Template.HP={TemplateHP}, IsAlive={IsAlive}", 
                    tower.Id, tower.TowerTemplate.Type, tower.TowerTemplate.Hp, tower.IsAlive());
            }

            var enemyTowers = arena.Towers[enemyUserId].Where(t => t.IsAlive()).ToList();
            _logger.LogDebug("Torres enemigas vivas encontradas: {Count}", enemyTowers.Count);

            // Buscar la torre enemiga más cercana
            foreach (var tower in enemyTowers)
            {
                double distance = CalculateDistance(troop, tower);
                _logger.LogDebug("Torre enemiga: {TowerId} a distancia {Distance}", tower.Id, distance);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTower = tower;
                }
            }

            // Retorna la torre o (null, 0.0)
            return (closestTower, closestTower is not null ? minDistance : 0.0);
        }
    }
}