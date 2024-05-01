using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Harmonify.Handlers;
using Harmonify.Models;
using Harmonify.Responses;

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
        GameId = game.Id
      };
      webSocketConnections.Add(connection);

      var response = new Response<string> { Type = ResponseType.NewPlayer, Data = playerGuid };
      await SendMessage(connection.WS, response);
      await ListenForMessages(connection);
    }

    public bool TryGetExistingConnection(
      string playerGuid,
      out WebSocketConnection? connection,
      out ResponseError<string>? response,
      out int statusCode
    )
    {
      connection = webSocketConnections.Find((conn) => conn.PlayerGuid == playerGuid);

      if (connection == null)
      {
        response = new ResponseError<string>
        {
          Type = ResponseType.NoPlayerInGame,
          ErrorMessage = "This player is not connected to any game"
        };
        statusCode = 404;
        return false;
      }

      if (connection.WS.State != WebSocketState.Closed)
      {
        response = new ResponseError<string>
        {
          Type = ResponseType.NoPlayerInGame,
          ErrorMessage = "This player is already connected"
        };
        statusCode = 409;
        return false;
      }

      response = null;
      statusCode = 200;
      return true;
    }

    public async Task Reconnect(WebSocketConnection connection, WebSocket ws)
    {
      connection.WS = ws;
      var response = new Response<string> { Type = ResponseType.Reconnected };
      await SendMessage(connection.WS, response);
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
      while (true)
      {
        var message = await ReadMessage(connection);

        if (message == null)
        {
          continue;
        }

        if (message.Type == ResponseType.ConnectionClosed)
        {
          break;
        }

        if (message is ResponseError<object>)
        {
          await SendMessage(connection.WS, message);
        }
        else if (message.Type == ResponseType.EndGame)
        {
          await EndGame(connection.GameId);
          return;
        }
        else
        {
          await SendToOtherPlayers(connection.PlayerGuid, connection.GameId, message);

          var response = new Response<string>
          {
            Type = ResponseType.Acknowledged,
            Data = "Message delivered"
          };
          await SendMessage(connection.WS, response);
        }
      }

      await HandleDisconnectFromClient(connection);
    }

    public async Task EndGame(string gameId)
    {
      await Task.WhenAll(
        webSocketConnections
          .FindAll((connection) => connection.GameId == gameId)
          .Select(
            async (connection) =>
            {
              await CloseSafely(connection.WS, "Game finished");
            }
          )
      );

      webSocketConnections.RemoveAll((connection) => connection.GameId == gameId);
      gameService.RemoveGame(gameId);
    }

    public async Task SendToPlayer(string playerGuid, string gameId, object message)
    {
      var connection = webSocketConnections.Find(
        (connection) => connection.PlayerGuid == playerGuid && connection.GameId == gameId
      );

      if (connection == null)
      {
        return;
      }

      await SendMessage(connection.WS, message);
    }

    public async Task SendToAllPlayers(string gameId, object message)
    {
      await Task.WhenAll(
        webSocketConnections
          .FindAll((connection) => connection.GameId == gameId)
          .Select(
            async (connection) =>
            {
              await SendMessage(connection.WS, message);
            }
          )
      );
    }

    public async Task SendToOtherPlayers(string senderGuid, string gameId, object message)
    {
      await Task.WhenAll(
        webSocketConnections
          .FindAll(
            (connection) => connection.PlayerGuid != senderGuid && connection.GameId == gameId
          )
          .Select(
            async (connection) =>
            {
              await SendMessage(connection.WS, message);
            }
          )
      );
    }

    private async Task HandleDisconnectFromClient(WebSocketConnection connection)
    {
      if (connection.WS.State != WebSocketState.Closed)
      {
        await CloseSafely(connection.WS);

        var isAnyPlayerConnected = webSocketConnections.Exists(
          (searchedConnection) =>
            searchedConnection.WS.State != WebSocketState.Closed
            && searchedConnection.GameId == connection.GameId
        );

        if (!isAnyPlayerConnected)
        {
          webSocketConnections.RemoveAll((conn) => conn.GameId == connection.GameId);
          gameService.RemoveGame(connection.GameId);
        }
      }
    }

    private static async Task SendMessage(WebSocket webSocket, object message)
    {
      byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, JsonHandler.jsonOptions);

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

    private static async Task<Response<object?>?> ReadMessage(WebSocketConnection connection)
    {
      var receiveResult = await connection.WS.ReceiveAsync(
        new ArraySegment<byte>(connection.Buffer),
        CancellationToken.None
      );

      if (receiveResult.CloseStatus.HasValue)
      {
        return new Response<object?> { Type = ResponseType.ConnectionClosed };
      }

      var jsonString = Encoding.UTF8.GetString(connection.Buffer);
      jsonString = jsonString.Replace("\0", string.Empty);
      Array.Clear(connection.Buffer);

      try
      {
        return JsonSerializer.Deserialize<Response<object?>>(
          jsonString.Trim(),
          JsonHandler.jsonOptions
        );
      }
      catch (Exception)
      {
        return new ResponseError<object?>
        {
          Type = ResponseType.IncorrectFormat,
          ErrorMessage = "Incorrect message format"
        };
      }
    }

    private static async Task CloseSafely(WebSocket webSocket, string message = "Disconnected")
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
}
