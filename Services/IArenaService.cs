using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IArenaService
    {
        Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers);
    }
}