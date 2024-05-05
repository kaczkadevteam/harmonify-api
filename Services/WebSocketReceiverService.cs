using System.Net.WebSockets;
using System.Text.Json;
using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class WebSocketReceiverService(
  IGameService gameService,
  IConnectionRepository connectionRepository,
  IWebSocketSenderService sender
) : IWebSocketReceiverService
{
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
    connectionRepository.Add(connection);

    await WebSocketHelper.SendMessage(connection.WS, firstMessage);
    await ListenForMessages(connection);
  }

  public bool TryGetExistingConnection(
    string playerGuid,
    out WebSocketConnection? connection,
    out MessageError? response,
    out int statusCode
  )
  {
    connection = connectionRepository.GetByPlayerGuid(playerGuid);

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
    await WebSocketHelper.SendMessage(connection.WS, response);
    await ListenForMessages(connection);
  }

  public string GetWsConnections()
  {
    string data = "";
    foreach (var item in connectionRepository.GetAll())
    {
      data = data + "{" + item.ToString() + "}\n\n";
    }
    return data;
  }

  public async Task ListenForMessages(WebSocketConnection connection)
  {
    while (true)
    {
      var message = await WebSocketHelper.ReadMessage(connection);

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
        await WebSocketHelper.SendMessage(connection.WS, message);
        continue;
      }

      if (!gameService.IsAuthorized(connection.GameId, connection.PlayerGuid, message.Type))
      {
        var response = new MessageError
        {
          Type = MessageType.Forbidden,
          ErrorMessage = "Insufficient permissions to perform this action"
        };
        await WebSocketHelper.SendMessage(connection.WS, response);
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
    if (message.Type == MessageType.StartGame && message is MessageWithData<GameStartedDto> msg)
    {
      if (gameService.TryStartGame(connection.GameId))
      {
        var response = new MessageWithData<GameStartedDto>
        {
          Type = MessageType.GameStarted,
          Data = msg.Data
        };

        await sender.SendToAllPlayers(connection.GameId, response);
      }
      else
      {
        var response = new MessageError
        {
          Type = MessageType.Conflict,
          ErrorMessage = "Game is already running"
        };

        await WebSocketHelper.SendMessage(connection.WS, response);
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

        await sender.SendToAllPlayers(connection.GameId, response);
      }
      else
      {
        var response = new MessageError
        {
          Type = MessageType.Conflict,
          ErrorMessage = "Round is not waiting to be started"
        };

        await WebSocketHelper.SendMessage(connection.WS, response);
      }
    }
  }

  private async Task HandleDisconnectFromClient(WebSocketConnection connection)
  {
    if (connection.WS.State != WebSocketState.Closed)
    {
      await WebSocketHelper.CloseSafely(connection.WS);

      var isAnyPlayerConnected = connectionRepository.IsAnyPlayerConnected(connection.GameId);

      if (!isAnyPlayerConnected)
      {
        connectionRepository.RemoveAllByGameId(connection.GameId);
        gameService.RemoveGame(connection.GameId);
      }
    }
  }

  public async Task EndGame(string gameId)
  {
    await Task.WhenAll(
      connectionRepository
        .GetAllByGameId(gameId)
        .Select(
          async (connection) =>
          {
            await WebSocketHelper.CloseSafely(connection.WS, "Game finished");
          }
        )
    );

    connectionRepository.RemoveAllByGameId(gameId);
    gameService.RemoveGame(gameId);
  }
}
