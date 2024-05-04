using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Harmonify.Handlers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class WebSocketService(IGameService gameService) : IWebSocketService
{
  private readonly List<WebSocketConnection> webSocketConnections = [];

  public async Task StartConnection(
    WebSocket webSocket,
    string gameId,
    string playerGuid,
    Message firstMessage
  )
  {
    var connection = new WebSocketConnection
    {
      WS = webSocket,
      PlayerGuid = playerGuid,
      GameId = gameId
    };
    webSocketConnections.Add(connection);

    await SendMessage(connection.WS, firstMessage);
    await ListenForMessages(connection);
  }

  public bool TryGetExistingConnection(
    string playerGuid,
    out WebSocketConnection? connection,
    out MessageError? response,
    out int statusCode
  )
  {
    connection = webSocketConnections.Find((conn) => conn.PlayerGuid == playerGuid);

    if (connection == null)
    {
      response = new MessageError
      {
        Type = MessageType.NoPlayerInGame,
        ErrorMessage = "This player is not connected to any game"
      };
      statusCode = 404;
      return false;
    }

    if (connection.WS.State != WebSocketState.Closed)
    {
      response = new MessageError
      {
        Type = MessageType.Conflict,
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
    var response = new Message { Type = MessageType.Reconnected };
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

      if (message.Type == MessageType.CloseConnection)
      {
        break;
      }

      if (message is MessageError)
      {
        await SendMessage(connection.WS, message);
        continue;
      }

      if (!gameService.IsAuthorized(connection.GameId, connection.PlayerGuid, message.Type))
      {
        var response = new MessageError
        {
          Type = MessageType.Forbidden,
          ErrorMessage = "Insufficient permissions to perform this action"
        };
        await SendMessage(connection.WS, response);
        continue;
      }

      if (message.Type == MessageType.EndGame)
      {
        await EndGame(connection.GameId);
        return;
      }

      await HandleIncomingMessage(connection, message);
    }

    await HandleDisconnectFromClient(connection);
  }

  public async Task HandleIncomingMessage(WebSocketConnection connection, Message message)
  {
    if (message.Type == MessageType.StartGame)
    {
      if (gameService.TryStartGame(connection.GameId))
      {
        var response = new MessageWithData<GameStartedDto>
        {
          Type = MessageType.GameStarted,
          Data = new GameStartedDto { }
        };

        await SendToAllPlayers(connection.GameId, response);
      }
      else
      {
        var response = new MessageError
        {
          Type = MessageType.Conflict,
          ErrorMessage = "Game is already running"
        };

        await SendMessage(connection.WS, response);
      }
    }
    else if (message.Type == MessageType.StartRound)
    {
      if (gameService.TryStartRound(connection.GameId))
      {
        var response = new MessageWithData<long>
        {
          Type = MessageType.RoundStarted,
          Data = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await SendToAllPlayers(connection.GameId, response);
      }
      else
      {
        var response = new MessageError
        {
          Type = MessageType.Conflict,
          ErrorMessage = "Round is not waiting to be started"
        };

        await SendMessage(connection.WS, response);
      }
    }
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

  public async Task SendToPlayer(string playerGuid, string gameId, Message message)
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

  public async Task SendToAllPlayers(string gameId, Message message)
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

  public async Task SendToOtherPlayers(string senderGuid, string gameId, Message message)
  {
    await Task.WhenAll(
      webSocketConnections
        .FindAll((connection) => connection.PlayerGuid != senderGuid && connection.GameId == gameId)
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

  private static async Task<Message?> ReadMessage(WebSocketConnection connection)
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
      return JsonSerializer.Deserialize<Message>(jsonString.Trim(), JsonHandler.jsonOptions);
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
