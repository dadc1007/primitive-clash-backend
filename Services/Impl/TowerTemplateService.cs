using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class TowerTemplateService(AppDbContext context) : ITowerTemplateService
    {
        private readonly AppDbContext _context = context;

        public Task<TowerTemplate> GetLeaderTowerTemplate() =>
                    GetTowerTemplate(TowerType.Leader.ToString());

        public Task<TowerTemplate> GetGuardianTowerTemplate() =>
            GetTowerTemplate(TowerType.Guardian.ToString());

        private async Task<TowerTemplate> GetTowerTemplate(string type)
        {
            return await _context.TowerTemplates
                .Where(tt => tt.Type.ToString() == type)
                .FirstOrDefaultAsync()
                ?? throw new TowerTemplateNotFoundException(type);
        }
    }
}