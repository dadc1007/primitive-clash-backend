using Microsoft.EntityFrameworkCore;

namespace PrimitiveClash.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}