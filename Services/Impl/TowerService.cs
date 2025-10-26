using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class TowerService(ITowerTemplateService towerTemplateService) : ITowerService
    {
        private readonly ITowerTemplateService _towerTemplateService = towerTemplateService;

        public async Task<Tower> CreateLeaderTower(Guid playerStateId)
        {
            TowerTemplate template = await _towerTemplateService.GetLeaderTowerTemplate();

            return new Tower(playerStateId, template);
        }

        public async Task<Tower> CreateGuardianTower(Guid playerStateId)
        {
            TowerTemplate template = await _towerTemplateService.GetGuardianTowerTemplate();

            return new Tower(playerStateId, template);
        }

        public async Task<Dictionary<Guid, List<Tower>>> CreateAllGameTowers(Guid player1StateId, Guid player2StateId)
        {
            var leaderTemplate = await _towerTemplateService.GetLeaderTowerTemplate();
            var guardianTemplate = await _towerTemplateService.GetGuardianTowerTemplate();

            var p1Towers = new List<Tower>
            {
                new(player1StateId, leaderTemplate),
                new(player1StateId, guardianTemplate),
                new(player1StateId, guardianTemplate)
            };

            var p2Towers = new List<Tower>
            {
                new(player2StateId, leaderTemplate),
                new(player2StateId, guardianTemplate),
                new(player2StateId, guardianTemplate)
            };

            return new Dictionary<Guid, List<Tower>>
            {
                { player1StateId, p1Towers },
                { player2StateId, p2Towers }
            };
        }
    }
}