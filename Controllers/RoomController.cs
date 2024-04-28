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
    IGameRepository _gamerepository = gameRepository;
    IPlayerRepository _playerrepository = playerRepository;

    [Route("room/{roomId}")]
    public async Task<ActionResult> JoinRoom(string roomId)
    {
      var playerGuid = HttpContext.Request.Headers["Auth"];
      var room = _gamerepository.GetGame(roomId);

      if (room == null)
      {
        Console.WriteLine("No room");
        return NotFound("Room not found");
      }

      if (room.Host.Guid == playerGuid)
      {
        Console.WriteLine("Reconnect host");
        return Ok("Reconnect host");
      }

      if (room.Players.Any(player => player.Guid == playerGuid))
      {
        Console.WriteLine("Reconnect");
        return Ok("Reconnect");
      }

      Player newPlayer = _playerrepository.Create();
      room.Players.Add(newPlayer);

      if (HttpContext.WebSockets.IsWebSocketRequest)
      {
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine(roomId);
        await WebSocketRoomService.StartConnection(webSocket, playerGuid, roomId);
      }
      else
      {
        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      }

      return Ok(
        new
        {
          result = "Connected",
          guid = newPlayer.Guid,
          rooms = _gamerepository.GetGames(),
          ws = WebSocketRoomService.getWsList()
        }
      );
    }
  }
}
