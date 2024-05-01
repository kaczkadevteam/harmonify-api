using Harmonify.Models;

namespace Harmonify.Services;

public interface IGameService
{
  public Game Create(Player host);
  public bool GameExists(string id);
  public void AddPlayer(string id, Player player);
  public void RemoveGame(string id);
  public void HandlePlayerReconnect(string playerGuid, string gameId);
}
