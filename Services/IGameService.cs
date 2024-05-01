using Harmonify.Models;

namespace Harmonify.Services
{
  public interface IGameService
  {
    public Player CreateAndAddNewPlayer(Game game);
    public void RemoveGame(string id);
    public void HandlePlayerReconnect(string playerGuid, string gameId);
  }
}
