using System.Text;
using Harmonify.Data;
using Harmonify.Models;
using Harmonify.WebSockets.Room;
using Microsoft.AspNetCore.Mvc;

namespace Harmonify.Controllers
{
  [ApiController]
  public class RoomController(IGameRepository gameRepository, IPlayerRepository playerRepository)
    : ControllerBase
  {
    readonly IGameRepository _gamerepository = gameRepository;
    readonly IPlayerRepository _playerrepository = playerRepository;

    [HttpGet("room/ws")]
    public IActionResult getWsRooms()
    {
      return Ok(WebSocketRoomService.getWsList());
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("room/{roomId}")]
    public async Task JoinRoom(string roomId)
    {
      var playerGuid = HttpContext.Request.Headers["Auth"];
      var room = _gamerepository.GetGame(roomId);

      if (room == null)
      {
        Console.WriteLine("No room");

        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Room not found");
        return;
      }

      if (room.Host.Guid == playerGuid)
      {
        Console.WriteLine("Reconnect host");

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect host");
        return;
      }

      if (room.Players.Any(player => player.Guid == playerGuid))
      {
        Console.WriteLine("Reconnect");
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect");
        return;
      }

      Player newPlayer = _playerrepository.Create();
      room.Players.Add(newPlayer);

      if (HttpContext.WebSockets.IsWebSocketRequest)
      {
        HttpContext.Response.Headers.Add("Auth", newPlayer.Guid); // setting header which will be send with response on websocket upgrade
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(); // sending 101 response, which requires to workaround ActionResult with managing raw responses
        Console.WriteLine(roomId);
        await WebSocketRoomService.StartConnection(webSocket, newPlayer.Guid, roomId);
        return;
      }
      else
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsync("Not websocket request");
        return;
      }
    }
  }
}
