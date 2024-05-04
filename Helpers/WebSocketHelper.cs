using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Helpers;

public static class WebSocketHelper
{
  public static async Task SendMessage(WebSocket webSocket, object message)
  {
    byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, JsonHelper.jsonOptions);

    if (webSocket.State != WebSocketState.Closed)
    {
      await webSocket.SendAsync(
        new ArraySegment<byte>(jsonBytes),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
    }
  }

  public static async Task<Message?> ReadMessage(WebSocketConnection connection)
  {
    var receiveResult = await connection.WS.ReceiveAsync(
      new ArraySegment<byte>(connection.Buffer),
      CancellationToken.None
    );

    if (receiveResult.CloseStatus.HasValue)
    {
      return new Message { Type = MessageType.CloseConnection };
    }

    var jsonString = Encoding.UTF8.GetString(connection.Buffer);
    jsonString = jsonString.Replace("\0", string.Empty);
    Array.Clear(connection.Buffer);

    try
    {
      return JsonSerializer.Deserialize<Message>(jsonString.Trim(), JsonHelper.jsonOptions);
    }
    catch (Exception)
    {
      return new MessageError
      {
        Type = MessageType.IncorrectFormat,
        ErrorMessage = "Incorrect message format"
      };
    }
  }

  public static async Task CloseSafely(WebSocket webSocket, string message = "Disconnected")
  {
    if (webSocket.State != WebSocketState.Closed)
    {
      await webSocket.CloseAsync(
        WebSocketCloseStatus.NormalClosure,
        "Game finished",
        CancellationToken.None
      );
    }
  }
}
