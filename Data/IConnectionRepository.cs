using Harmonify.Models;

namespace Harmonify.Data;

public interface IConnectionRepository
{
  void Add(WebSocketConnection connection);
  WebSocketConnection? GetByPlayerGuid(string playerGuid);
  List<WebSocketConnection> GetAllByGameId(string gameId);
  List<WebSocketConnection> GetAll();
  void RemoveByPlayerGuid(string playerGuid);
  void RemoveAllByGameId(string gameId);
  bool IsAnyPlayerConnected(string gameId);
}
