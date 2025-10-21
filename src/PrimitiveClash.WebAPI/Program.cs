using PrimitiveClash.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => new
{
    Name = "Primitive Clash WebAPI",
    Version = "0.1.0",
    Description = "Web API project (Clean Architecture)"
});

app.MapControllers();

// SignalR hub para tiempo real (WebSockets cuando esté disponible)
app.MapHub<GameHub>("/gameHub");

app.Run();
