using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PrimitiveClash.Backend.Background;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Factories;
using PrimitiveClash.Backend.Services.Factories.Impl;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// Cors configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "CorsPolicy",
        builder =>
            builder
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:4173",
                    "https://primitive-clash-frontend.vercel.app"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
    );
});

// Authentication configuration
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        options =>
        {
            builder.Configuration.Bind("AzureAd", options);
            options.TokenValidationParameters.ValidateIssuer = false;

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
            };
        },
        options =>
        {
            builder.Configuration.Bind("AzureAd", options);
        }
    );

builder.Services.AddAuthorization();

// DB context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configuration
builder.Services.Configure<GameSettings>(builder.Configuration.GetSection("GameSettings"));

// Redis configuration
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString!)
);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

// Dependency Injection for services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<IPlayerCardService, PlayerCardService>();
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
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddSingleton<IGameLoopService, GameLoopService>();
builder.Services.AddHostedService<GameLoopWorker>();
builder.Services.AddHealthChecks();

// SignalR Service
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString!);
builder.Services.AddHostedService<MatchmakingService>();
builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();

// Add services to the container.
builder
    .Services.AddControllers()
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
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MatchmakingHub>("/hubs/matchmaking");
app.MapHub<GameHub>("/hubs/game");
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
