using System.Net.WebSockets;
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
    await gameService.SendPlayerList(gameId);
    try
    {
      await ListenForMessages(connection);
    }
    catch (Exception) { }
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

    if (
      connection.WS.State != WebSocketState.Closed
      && connection.WS.State != WebSocketState.Aborted
    )
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
    try
    {
      await ListenForMessages(connection);
    }
    catch (Exception) { }
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
        await gameService.EndGame(connection.GameId);
        return;
      }

      if (message.Type == MessageType.PauseGame)
      {
        await gameService.PauseGame(connection.GameId, connection.PlayerGuid);
      }

      if (message.Type == MessageType.UnPauseGame)
      {
        await gameService.UnPauseGame(connection.GameId, connection.PlayerGuid);
      }

      await HandleIncomingMessage(connection, message);
    }

    await HandleDisconnectFromClient(connection);
  }

  public async Task HandleIncomingMessage(WebSocketConnection connection, Message message)
  {
    if (message.Type == MessageType.StartGame && message is MessageWithData<StartGameDto> msg)
    {
      if (
        gameService.TryStartGame(
          connection.GameId,
          msg.Data,
          out var timestamp,
          out var uri,
          out var trackStart_ms
        )
      )
      {
        var response = new MessageWithData<GameStartedDto>
        {
          Type = MessageType.GameStarted,
          Data = new GameStartedDto
          {
            GameSettings = msg.Data.GameSettings,
            RoundStartTimestamp = timestamp,
            PossibleGuesses = msg
              .Data.Tracks.Select(
                (track) => new DisplayedGuessDto { Guess = track.Guess, Id = track.Uri }
              )
              .ToList(),
            Uri = uri,
            TrackStart_ms = trackStart_ms
          }
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
    else if (message.Type == MessageType.Guess && message is MessageWithData<string> msg2)
    {
      if (gameService.TryEvaluatePlayerGuess(connection.GameId, connection.PlayerGuid, msg2.Data))
      {
        var response = new Message { Type = MessageType.Acknowledged };

        await WebSocketHelper.SendMessage(connection.WS, response);
        await gameService.TryEndRoundIfAllGuessessSubmitted(connection.GameId);
      }
    }
    else if (
      message.Type == MessageType.ChangeName
      && message is MessageWithData<string> newNickname
    )
    {
      if (gameService.TryChangeName(connection.GameId, connection.PlayerGuid, newNickname.Data))
      {
        await gameService.SendPlayerList(connection.GameId);
      }
      else
      {
        var response = new MessageError
        {
          Type = MessageType.Conflict,
          ErrorMessage = "Name couldn't be changed"
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
        await gameService.EndGame(connection.GameId);
      }
    }
  }
}
