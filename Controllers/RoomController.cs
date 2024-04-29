using Harmonify.Data;
using Harmonify.Services;
using Microsoft.AspNetCore.Mvc;

namespace Harmonify.Controllers
{
  [ApiController]
  public class RoomController(IGameRepository gameRepository, IWebSocketService webSocketService)
    : ControllerBase
  {
    readonly IGameRepository gameRepository = gameRepository;
    readonly IWebSocketService webSocketService = webSocketService;

    [HttpGet("room/ws")]
    public IActionResult GetWsRooms()
    {
      return Ok(webSocketService.GetWsList());
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("room/{roomId}")]
    public async Task JoinRoom(string roomId)
    {
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsync("Not websocket request");
        return;
      }

      var playerGuid = HttpContext.Request.Query["reconnect"];
      var game = gameRepository.GetGame(roomId);

      if (game == null)
      {
        Console.WriteLine($"No room {roomId}");

        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Room not found");
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
  }
}
