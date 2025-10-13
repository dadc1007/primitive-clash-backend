using Microsoft.EntityFrameworkCore;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

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

// SignalR Service
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString!);
builder.Services.AddHostedService<MatchmakingService>();
builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapHub<MatchmakingHub>("/matchmaking");
app.MapControllers();
app.Run();
