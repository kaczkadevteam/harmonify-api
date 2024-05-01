using System.Diagnostics;
using Harmonify.Data;
using Harmonify.Responses;
using Harmonify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Harmonify.Controllers
{
  [ApiController]
  public class GameController(
    IGameService gameService,
    IGameRepository gameRepository,
    IPlayerRepository playerRepository,
    IWebSocketService webSocketService
  ) : ControllerBase
  {
    readonly IGameService gameService = gameService;
    readonly IGameRepository gameRepository = gameRepository;
    readonly IPlayerRepository playerRepository = playerRepository;

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("create")]
    public async Task CreateGame()
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        await RespondNotWebSocketConnection();
        return;
      }

      var player = playerRepository.Create();
      var game = gameRepository.Create(player);

      var createdGame = new Response<object>
      {
        Type = ResponseType.CreatedGame,
        Data = new CreatedGameDto { HostGuid = player.Guid, GameId = game.Id }
      };
      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, game.Id, player.Guid, createdGame);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("game/{id}")]
    public async Task JoinGame(string id)
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        await RespondNotWebSocketConnection();
        return;
      }

      var game = gameRepository.GetGame(id);
      if (game == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Game not found");
        return;
      }

      var playerGuid = gameService.CreateAndAddNewPlayer(game).Guid;
      var response = new Response<object> { Type = ResponseType.NewPlayer, Data = playerGuid };

      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, game.Id, playerGuid, response);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("game/reconnect/{playerGuid}")]
    public async Task ReconnectPlayer(string playerGuid)
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        await RespondNotWebSocketConnection();
        return;
      }

      if (
        webSocketService.TryGetExistingConnection(
          playerGuid,
          out var connection,
          out var response,
          out var statusCode
        )
      )
      {
        using var newWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await webSocketService.Reconnect(connection!, newWebSocket);
      }
      else
      {
        HttpContext.Response.StatusCode = statusCode;
        await HttpContext.Response.WriteAsJsonAsync(response);
      }
    }

    [HttpGet("game/ws")]
    public IActionResult GetWsConnections()
    {
      var response = new Response<string>
      {
        Type = ResponseType.ConnectionsList,
        Data = webSocketService.GetWsConnections()
      };
      return Ok(response);
    }

    private async Task RespondNotWebSocketConnection()
    {
      HttpContext.Response.StatusCode = 426;
      HttpContext.Response.Headers.Append("Upgrade", "websocket");
      await HttpContext.Response.WriteAsync("Not websocket request");
    }
  }
}
