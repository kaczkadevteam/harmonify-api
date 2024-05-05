using System.Net.WebSockets;
using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class WebSocketSenderService(IConnectionRepository connectionRepository)
  : IWebSocketSenderService
{
  public async Task SendToPlayer(string playerGuid, string gameId, Message message)
  {
    var connection = connectionRepository.GetByPlayerGuid(playerGuid);

    if (connection == null)
    {
      return;
    }

    await WebSocketHelper.SendMessage(connection.WS, message);
  }

  public async Task SendToAllPlayers(string gameId, Message message)
  {
    await Task.WhenAll(
      connectionRepository
        .GetAllByGameId(gameId)
        .Select(
          async (connection) =>
          {
            await WebSocketHelper.SendMessage(connection.WS, message);
          }
        )
    );
  }

  public async Task SendToOtherPlayers(string senderGuid, string gameId, Message message)
  {
    await Task.WhenAll(
      connectionRepository
        .GetAllByGameId(gameId)
        .FindAll((connection) => connection.PlayerGuid != senderGuid)
        .Select(
          async (connection) =>
          {
            await WebSocketHelper.SendMessage(connection.WS, message);
          }
        )
    );
  }

  public async Task EndConnections(string gameId)
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
  }
}
