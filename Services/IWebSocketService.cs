using System.Net.WebSockets;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public interface IWebSocketService
{
  public Task StartConnection(
    WebSocket webSocket,
    string gameId,
    string playerGuid,
    Message firstMessage
  );
  public Task Reconnect(WebSocketConnection connection, WebSocket webSocket);
  public bool TryGetExistingConnection(
    string playerGuid,
    out WebSocketConnection? connection,
    out MessageError? response,
    out int statusCode
  );
  public string GetWsConnections();
}
