using Harmonify.Data;
using Harmonify.Handlers;
using Harmonify.Helpers;
using Harmonify.Services;

var builder = WebApplication.CreateBuilder(args);

builder
  .Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.Converters.Add(JsonHelper.enumConverter);
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonHelper
      .jsonOptions
      .PropertyNamingPolicy;
  });
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IConnectionRepository, ConnectionRepository>();

builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IWebSocketReceiverService, WebSocketReceiverService>();
builder.Services.AddSingleton<IWebSocketSenderService, WebSocketSenderService>();

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
