using Harmonify.Data;
using Harmonify.Models;

namespace Harmonify.Services
{
  public class GameService(IGameRepository gameRepository) : IGameService
  {
    readonly IGameRepository gameRepository = gameRepository;

    public void AddPlayer(string id, Player player)
    {
      gameRepository.GetGame(id)?.Players.Add(player);
    }

    public void HandlePlayerReconnect(string playerGuid, string gameId)
    {
      throw new NotImplementedException();
    }

    public void RemoveGame(string id)
    {
      gameRepository.RemoveGame(id);
    }
  }
}
