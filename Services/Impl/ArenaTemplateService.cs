using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class ArenaTemplateService(AppDbContext context) : IArenaTemplateService
    {
        private readonly AppDbContext _context = context;

        public async Task<ArenaTemplate> GetDefaultArenaTemplate()
        {
            return await _context.ArenaTemplates
                .FirstOrDefaultAsync()
                ?? throw new DefaultArenaTemplateNotFoundException();
        }
    }
}