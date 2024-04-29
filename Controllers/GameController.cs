using Harmonify.Data;
using Harmonify.Models;
using Harmonify.Services;
using Microsoft.AspNetCore.Mvc;

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

      var createdGame = new CreatedGameDto { HostGuid = player.Guid, GameId = game.Id };

      return Ok(createdGame);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("room/{id}")]
    public async Task JoinGame(string id)
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsync("Not websocket request");
        return;
      }

      //FIXME: If there is guid but it isn't in the game there should be error instead of starting connection?
      var playerGuid = HttpContext.Request.Query["reconnect"];
      var game = gameRepository.GetGame(id);

      if (game == null)
      {
        Console.WriteLine($"No game {id}");

        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Game not found");
        return;
      }

      //TODO: Verify if this player isn't already in the game
      if (game.Host.Guid == playerGuid)
      {
        Console.WriteLine("Reconnect host");

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect host");
        return;
      }

      //TODO: Verify if this player isn't already in the game
      if (game.Players.Any(player => player.Guid == playerGuid))
      {
        Console.WriteLine("Reconnect");
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect");
        return;
      }

      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, game);
    }

    [HttpGet("room/ws")]
    public IActionResult GetWsConnections()
    {
      return Ok(webSocketService.GetWsConnections());
    }
  }
}
