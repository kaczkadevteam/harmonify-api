using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Harmonify.Models;

namespace Harmonify.Services
{
  public class WebSocketService(IGameService gameService) : IWebSocketService
  {
    private readonly IGameService gameService = gameService;
    private readonly List<WebSocketConnection> webSocketConnections = [];

    public async Task StartConnection(WebSocket webSocket, Game game)
    {
      var playerGuid = gameService.AddNewPlayer(game).Guid;

      var connection = new WebSocketConnection
      {
        WS = webSocket,
        PlayerGuid = playerGuid,
        RoomId = game.RoomId
      };
      webSocketConnections.Add(connection);

      foreach (var item in webSocketConnections)
      {
        Console.WriteLine(item.ToString());
      }

      await SendMessage(connection.WS, $"Hello player {webSocketConnections.Count}");
      await ListenForMessages(connection);
    }

    public string GetWsConnections()
    {
      string data = "";
      foreach (var item in webSocketConnections)
      {
        data = data + "{" + item.ToString() + "}\n\n";
      }
      return data;
    }

    public async Task ListenForMessages(WebSocketConnection connection)
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult receiveResult;

      try
      {
        receiveResult = await connection.WS.ReceiveAsync(
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
        var jsonString = Encoding.UTF8.GetString(buffer);
        jsonString = jsonString.Replace("\0", string.Empty);
        Console.WriteLine(jsonString);
        var message = JsonSerializer.Deserialize<object>(jsonString.Trim());
        Console.WriteLine(jsonString);
        Array.Clear(buffer);

        if (message != null)
        {
          await SendToOtherPlayers(connection.PlayerGuid, message);
        }

        try
        {
          receiveResult = await connection.WS.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None
          );
        }
        catch (Exception)
        {
          return;
        }
      }
      if (connection.WS.State != WebSocketState.Closed)
      {
        await connection.WS.CloseAsync(
          receiveResult.CloseStatus.Value,
          receiveResult.CloseStatusDescription,
          CancellationToken.None
        );
        webSocketConnections.Remove(connection);
      }
    }

    public async Task EndGame()
    {
      await Task.WhenAll(
        webSocketConnections.Select(
          async (connection) =>
          {
            await connection.WS.CloseAsync(
              WebSocketCloseStatus.NormalClosure,
              "Game finished",
              CancellationToken.None
            );
          }
        )
      );

      webSocketConnections.Clear();
    }

    public async Task SendToPlayer(string playerGuid, object message)
    {
      var player = webSocketConnections.Find((connection) => connection.PlayerGuid == playerGuid);

      if (player == null)
      {
        return;
      }

      await SendMessage(player.WS, message);
    }

    public async Task SendToAllPlayers(object message)
    {
      await Task.WhenAll(
        webSocketConnections.Select(
          async (connection) =>
          {
            await SendMessage(connection.WS, message);
          }
        )
      );
    }

    public async Task SendToOtherPlayers(string senderGuid, object message)
    {
      await Task.WhenAll(
        webSocketConnections
          .FindAll((connection) => connection.PlayerGuid != senderGuid)
          .Select(
            async (connection) =>
            {
              await SendMessage(connection.WS, message);
            }
          )
      );
    }

    private static async Task SendMessage(WebSocket webSocket, object message)
    {
      byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message);

      await webSocket.SendAsync(
        new ArraySegment<byte>(jsonBytes),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
    }
  }
}
