using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaService(IArenaTemplateService arenaTemplateService) : IArenaService
    {
        private readonly IArenaTemplateService _arenaTemplateService = arenaTemplateService;

        public async Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers)
        {
            ArenaTemplate arenaTemplate = await _arenaTemplateService.GetDefaultArenaTemplate();

            return new Arena(arenaTemplate, towers);
        }
    }
}