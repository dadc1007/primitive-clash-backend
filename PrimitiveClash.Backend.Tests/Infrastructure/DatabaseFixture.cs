// using Microsoft.EntityFrameworkCore;
// using PrimitiveClash.Backend.Data;
// using Testcontainers.PostgreSql;

// namespace PrimitiveClash.Backend.Tests.Infrastructure;

// public class DatabaseFixture : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
//         .WithImage("postgres:15")
//         .WithDatabase("testdb")
//         .WithUsername("testuser")
//         .WithPassword("testpass")
//         .Build();

//     public AppDbContext CreateContext()
//     {
//         var options = new DbContextOptionsBuilder<AppDbContext>()
//             .UseNpgsql(_container.GetConnectionString())
//             .Options;

//         var context = new AppDbContext(options);

//         // Limpiar la base de datos antes de cada uso
//         context.Database.EnsureDeleted();
//         context.Database.EnsureCreated();

//         return context;
//     }

//     public async Task InitializeAsync()
//     {
//         await _container.StartAsync();
//     }

//     public async Task DisposeAsync()
//     {
//         await _container.DisposeAsync();
//     }
// }
