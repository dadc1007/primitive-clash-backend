using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Cors configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:5173")
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
builder.Services.AddScoped<IGameService, GameService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHub<MatchmakingHub>("/hubs/matchmaking");
app.MapHub<GameHub>("/hubs/Game");
app.MapControllers();
app.Run();
