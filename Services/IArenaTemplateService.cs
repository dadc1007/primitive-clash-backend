using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public interface IArenaTemplateService
    {
        Task<ArenaTemplate> GetDefaultArenaTemplate();
    }
}