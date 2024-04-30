using Harmonify.Data;
using Harmonify.Models;

namespace Harmonify.Services
{
  public class GameService(IGameRepository gameRepository, IPlayerRepository playerRepository)
    : IGameService
  {
    readonly IGameRepository gameRepository = gameRepository;
    readonly IPlayerRepository playerRepository = playerRepository;

    public Player AddNewPlayer(Game game)
    {
      var player = playerRepository.Create();
      game.Players.Add(player);
      return player;
    }
  }
}