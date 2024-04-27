using Harmonify.Data;
using Harmonify.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harmonify.Controllers
{
  [ApiController]
  public class GameController(IGameRepository gameRepository, IPlayerRepository playerRepository)
    : ControllerBase
  {
    readonly IGameRepository repository = gameRepository;
    readonly IPlayerRepository playerRepository = playerRepository;

    [HttpGet("create")]
    public ActionResult CreateRoom()
    {
      var player = playerRepository.Create();
      var game = repository.Create(player);

      var createdRoom = new CreatedRoomDto { HostGuid = player.Guid, RoomId = game.RoomId };

      return Ok(createdRoom);
    }
  }
}
