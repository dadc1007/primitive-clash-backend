using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Factories.Impl;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Cors configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:5173", "http://localhost:4173", "https://primitive-clash-frontend.vercel.app")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// DB context
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration    
builder.Services.Configure<GameSettings>(builder.Configuration.GetSection("GameSettings"));

// Redis configuration
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString!)
);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
);

// Dependency Injection for services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<IPlayerCardService, PlayerCardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerStateService, PlayerStateService>();
builder.Services.AddScoped<ITowerTemplateService, TowerTemplateService>();
builder.Services.AddScoped<ITowerService, TowerService>();
builder.Services.AddScoped<IArenaTemplateService, ArenaTemplateService>();
builder.Services.AddScoped<IArenaService, ArenaService>();
builder.Services.AddScoped<IPathfindingService, PathfindingService>();
builder.Services.AddScoped<IArenaEntityFactory, ArenaEntityFactory>();
builder.Services.AddScoped<IBattleService, BattleService>();
builder.Services.AddScoped<IBehaviorService, BehaviourService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IGameLoopService, GameLoopService>();
builder.Services.AddHostedService<GameLoopWorker>();

// SignalR Service
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString!);
builder.Services.AddHostedService<MatchmakingService>();
builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Aplicando migraciones de base de datos...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        DbSeeder.Seed(db);
        logger.LogInformation("Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones de base de datos");

        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHub<MatchmakingHub>("/hubs/matchmaking");
app.MapHub<GameHub>("/hubs/game");
app.MapControllers();
app.Run();
