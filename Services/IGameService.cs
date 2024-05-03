using Harmonify.Models;
using Harmonify.Responses;

namespace Harmonify.Services;

public interface IGameService
{
  public Game Create(Player host);
  public bool GameExists(string id);
  public bool TryStartGame(string id);
  public void AddPlayer(string id, Player player);
  public void RemoveGame(string id);
  public void HandlePlayerReconnect(string playerGuid, string gameId);
  public bool IsAuthorized(string gameId, string playerGuid, ResponseType messageType);
}
