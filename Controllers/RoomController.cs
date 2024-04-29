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
      if (!HttpContext.WebSockets.IsWebSocketRequest)
      {
        HttpContext.Response.StatusCode = 400;
        await HttpContext.Response.WriteAsync("Not websocket request");
        return;
      }

      var playerGuid = HttpContext.Request.Query["reconnect"];
      var room = _gamerepository.GetGame(roomId);

      if (room == null)
      {
        Console.WriteLine($"No room {roomId}");

        HttpContext.Response.StatusCode = 404;
        await HttpContext.Response.WriteAsync("Room not found");
        return;
      }

      //TODO: Verify if this player isn't already in the game
      if (room.Host.Guid == playerGuid)
      {
        Console.WriteLine("Reconnect host");

        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect host");
        return;
      }

      //TODO: Verify if this player isn't already in the game
      if (room.Players.Any(player => player.Guid == playerGuid))
      {
        Console.WriteLine("Reconnect");
        HttpContext.Response.StatusCode = 200;
        await HttpContext.Response.WriteAsync("Reconnect");
        return;
      }

      Player newPlayer = _playerrepository.Create();
      room.Players.Add(newPlayer);

      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await WebSocketRoomService.StartConnection(webSocket, newPlayer.Guid, roomId);
    }
  }
}
