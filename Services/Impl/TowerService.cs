using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class TowerService(ITowerTemplateService towerTemplateService) : ITowerService
    {
        private readonly ITowerTemplateService _towerTemplateService = towerTemplateService;

        public async Task<Tower> CreateLeaderTower(Guid playerStateId)
        {
            TowerTemplate template = await _towerTemplateService.GetLeaderTowerTemplate();

            return new Tower(template, playerStateId);
        }

        public async Task<Tower> CreateGuardianTower(Guid playerStateId)
        {
            TowerTemplate template = await _towerTemplateService.GetGuardianTowerTemplate();

            return new Tower(template, playerStateId);
        }

        public async Task<Dictionary<Guid, List<Tower>>> CreateAllGameTowers(Guid player1StateId, Guid player2StateId)
        {
            var leaderTemplate = await _towerTemplateService.GetLeaderTowerTemplate();
            var guardianTemplate = await _towerTemplateService.GetGuardianTowerTemplate();

            var p1Towers = new List<Tower>
            {
                new(leaderTemplate, player1StateId),
                new(guardianTemplate, player1StateId),
                new(guardianTemplate, player1StateId)
            };

            var p2Towers = new List<Tower>
            {
                new(leaderTemplate, player2StateId),
                new(guardianTemplate, player2StateId),
                new(guardianTemplate, player2StateId)
            };

            return new Dictionary<Guid, List<Tower>>
            {
                { player1StateId, p1Towers },
                { player2StateId, p2Towers }
            };
        }
    }
}