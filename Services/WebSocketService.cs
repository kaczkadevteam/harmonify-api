using System.Net.WebSockets;
using System.Text;
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

      await webSocket.SendAsync(
        new ArraySegment<byte>(
          Encoding.UTF8.GetBytes($"Hello player {webSocketConnections.Count}")
        ),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );

      await ListenForMessages(connection);
    }

    public string GetWsList()
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
        var message = Encoding.UTF8.GetString(buffer);
        Console.WriteLine(message);
        Array.Clear(buffer);

        //TODO: Can't make this comparison to work
        if (!message.Trim().Equals(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("end"))))
        {
          await SendToOtherPlayers(connection.PlayerGuid, message);
        }
        else
        {
          await EndGame();
          Console.WriteLine("Finish");
          return;
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

    public async Task SendToOtherPlayers(string senderGuid, string message)
    {
      await Task.WhenAll(
        webSocketConnections
          .FindAll((player) => player.PlayerGuid != senderGuid)
          .Select(
            async (player) =>
            {
              await player.WS.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
              );
            }
          )
      );
    }

    public async Task EndGame()
    {
      await Task.WhenAll(
        webSocketConnections.Select(
          async (player) =>
          {
            await player.WS.CloseAsync(
              WebSocketCloseStatus.NormalClosure,
              "Game finished",
              CancellationToken.None
            );
          }
        )
      );

      webSocketConnections.Clear();
    }
  }
}
