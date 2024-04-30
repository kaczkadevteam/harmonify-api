using Harmonify.Data;
using Harmonify.Handlers;
using Harmonify.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseWebSockets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseExceptionHandler(_ => { });
app.MapControllers();

app.Run();
