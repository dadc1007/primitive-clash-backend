using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface ITowerTemplateService
    {
        Task<TowerTemplate> GetLeaderTowerTemplate();
        Task<TowerTemplate> GetGuardianTowerTemplate();
    }
}