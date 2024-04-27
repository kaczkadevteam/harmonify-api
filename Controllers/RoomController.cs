using Harmonify.Data;
using Harmonify.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harmonify.Controllers
{
  [ApiController]
  public class RoomController(IGameRepository gameRepository, IPlayerRepository playerRepository)
    : ControllerBase
  {
    IGameRepository _gamerepository = gameRepository;
    IPlayerRepository _playerrepository = playerRepository;

    [HttpPost("room/{id}")]
    public ActionResult JoinRoom(string id)
    {
      var playerGuid = HttpContext.Request.Headers["Auth"];
      var room = _gamerepository.GetGame(id);
      if (room == null)
      {
        Console.WriteLine("No room");
        return NotFound("Room not found");
      }
      if (room.Players.Any(player => player.Guid == playerGuid))
      {
        Console.WriteLine("Reconnect");
        return Ok("Reconnect");
      }
      Player newPlayer = _playerrepository.Create();
      room.Players.Add(newPlayer);
      return Ok(
        new
        {
          result = "Connected",
          guid = newPlayer.Guid,
          rooms = _gamerepository.GetGames()
        }
      );
    }
  }
}
