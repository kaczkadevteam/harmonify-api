using System.Net.WebSockets;
using System.Text;

namespace Harmonify.WebSockets.Room
{
  public class WebSocketRoomService
  {
    private static List<WebSocketPlayer> webSocketPlayers = [];

    public static async Task StartConnection(WebSocket webSocket, string guid, string roomId)
    {
      var player = new WebSocketPlayer(webSocket, guid, roomId);
      webSocketPlayers.Add(player);
      Console.WriteLine(player);
      Console.WriteLine(webSocketPlayers);

      await webSocket.SendAsync(
        new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Hello player {webSocketPlayers.Count}")),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
      await webSocket.SendAsync(
        new ArraySegment<byte>(Encoding.UTF8.GetBytes(player.guid)),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
      Console.WriteLine(webSocketPlayers);
    }

    public static List<WebSocketPlayer> getWsList()
    {
      return webSocketPlayers;
    }

    public static async Task ListenForMessages(WebSocketPlayer webSocketPlayer)
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult receiveResult;

      try
      {
        receiveResult = await webSocketPlayer.webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer),
          CancellationToken.None
        );
      }
      catch (Exception)
      {
        return;
      }

      while (!receiveResult.CloseStatus.HasValue)
      {
        var message = Encoding.UTF8.GetString(buffer);
        Console.WriteLine(message);
        Array.Clear(buffer);

        //TODO: Can't make this comparison to work
        if (!message.Trim().Equals(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("end"))))
        {
          //   await SendToOtherPlayers(webSocketPlayer.guid, message);
        }
        else
        {
          //TODO: Calling this method throws error even though all ReceiveAsync are in try-catch
          //   await EndGame();
          Console.WriteLine("Finish");
          return;
        }

        try
        {
          receiveResult = await webSocketPlayer.webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None
          );
        }
        catch (Exception)
        {
          return;
        }
      }
      if (webSocketPlayer.webSocket.State != WebSocketState.Closed)
      {
        await webSocketPlayer.webSocket.CloseAsync(
          receiveResult.CloseStatus.Value,
          receiveResult.CloseStatusDescription,
          CancellationToken.None
        );
        webSocketPlayers.Remove(webSocketPlayer);
      }
    }
  }
}