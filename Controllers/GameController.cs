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
    IGameRepository gameRepository,
    IPlayerRepository playerRepository,
    IWebSocketService webSocketService
  ) : ControllerBase
  {
    readonly IGameRepository gameRepository = gameRepository;
    readonly IPlayerRepository playerRepository = playerRepository;

    [HttpPost("create")]
    public ActionResult CreateGame()
    {
      var player = playerRepository.Create();
      var game = gameRepository.Create(player);

      var createdGame = new Response<CreatedGameDto>
      {
        Type = ResponseType.CreatedRoom,
        Data = new CreatedGameDto { HostGuid = player.Guid, GameId = game.Id }
      };

      return Ok(createdGame);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("game/{id}")]
    public async Task JoinGame(string id)
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsync("Not websocket request");
        return;
      }

      var game = gameRepository.GetGame(id);
      if (game == null)
      {
        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Game not found");
        return;
      }

      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, game);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("game/reconnect/{playerGuid}")]
    public async Task ReconnectPlayer(string playerGuid)
    {
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
  }
}
