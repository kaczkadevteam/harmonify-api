using Harmonify.Models;

namespace Harmonify.Services;

public interface IGameService
{
  Game Create(Player host);
  bool GameExists(string id);
  GameState? GetStateIfExists(string id);
  Task QuitGame(string gameId, string playerGuid);
  Task EndGame(string id);
  Task RemoveDisconnectedPlayers(Game game);
  Task RemoveGameAndConnections(string gameId);
  Task SendPlayerList(string gameId);
}
