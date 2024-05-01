using Harmonify.Data;
using Harmonify.Models;
using Harmonify.Responses;
using Harmonify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Harmonify.Controllers
{
  [ApiController]
  public class GameController(IGameService gameService, IWebSocketService webSocketService)
    : ControllerBase
  {
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("create")]
    public async Task CreateGame()
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        await RespondNotWebSocketConnection();
        return;
      }

      var player = new Player();
      var game = gameService.Create(player);

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

      if (!gameService.GameExists(id))
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsJsonAsync(
          new ResponseError<string>
          {
            Type = ResponseType.GameDoesntExist,
            ErrorMessage = "Game not found"
          }
        );
        return;
      }

      var player = new Player();
      gameService.AddPlayer(id, player);

      var response = new Response<object> { Type = ResponseType.NewPlayer, Data = player.Guid };
      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, id, player.Guid, response);
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
