using Harmonify.Data;
using Harmonify.Models;
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

      var playerGuid = HttpContext.Request.Query["reconnect"];
      if (playerGuid != StringValues.Empty)
      {
        //TODO: Verify if this player isn't already in the game
        if (game.Host.Guid == playerGuid)
        {
          throw new NotImplementedException("Reconnect host");
        }
        //TODO: Verify if this player isn't already in the game
        else if (game.Players.Any(player => player.Guid == playerGuid))
        {
          throw new NotImplementedException("Reconnect player");
        }
        else
        {
          //FIXME: Isn't there better status to represent this error?
          HttpContext.Response.StatusCode = 400;
          var response = new ResponseError<object>
          {
            Type = ResponseType.NoPlayerInGame,
            ErrorMessage = "Couldn't find this player in this game"
          };
          await HttpContext.Response.WriteAsJsonAsync(response);
        }
      }

      using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
      await webSocketService.StartConnection(webSocket, game);
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
