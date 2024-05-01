using Harmonify.Models;

namespace Harmonify.Services
{
  public interface IGameService
  {
    public void AddPlayer(string id, Player player);
    public void RemoveGame(string id);
    public void HandlePlayerReconnect(string playerGuid, string gameId);
  }
}
