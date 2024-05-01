using System.Net.WebSockets;
using Harmonify.Models;
using Harmonify.Responses;

namespace Harmonify.Services;

public interface IWebSocketService
{
  public Task StartConnection(
    WebSocket webSocket,
    string gameId,
    string playerGuid,
    Response<object> firstMessage
  );
  public Task Reconnect(WebSocketConnection connection, WebSocket webSocket);
  public bool TryGetExistingConnection(
    string playerGuid,
    out WebSocketConnection? connection,
    out ResponseError<string>? response,
    out int statusCode
  );
  public string GetWsConnections();
}
