using System.Net.WebSockets;
using Harmonify.Models;

namespace Harmonify.Data;

public class ConnectionRepository : IConnectionRepository
{
  private readonly List<WebSocketConnection> webSocketConnections = [];

  public void Add(WebSocketConnection connection)
  {
    webSocketConnections.Add(connection);
  }

  public WebSocketConnection? GetByPlayerGuid(string playerGuid)
  {
    return webSocketConnections.Find((conn) => conn.PlayerGuid == playerGuid);
  }

  public List<WebSocketConnection> GetAllByGameId(string gameId)
  {
    return webSocketConnections.FindAll((connection) => connection.GameId == gameId);
  }

  public List<WebSocketConnection> GetAll()
  {
    return webSocketConnections;
  }

  public void RemoveByPlayerGuid(string playerGuid)
  {
    var connection = webSocketConnections.Find((conn) => conn.PlayerGuid == playerGuid);

    if (connection != null)
    {
      webSocketConnections.Remove(connection);
    }
  }

  public void RemoveAllByGameId(string gameId)
  {
    webSocketConnections.RemoveAll((conn) => conn.GameId == gameId);
  }

  public bool IsAnyPlayerConnected(string gameId)
  {
    return webSocketConnections.Exists(
      (searchedConnection) =>
        searchedConnection.WS.State != WebSocketState.Closed && searchedConnection.GameId == gameId
    );
  }
}
