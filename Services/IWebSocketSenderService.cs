using Harmonify.Messages;

namespace Harmonify.Services;

public interface IWebSocketSenderService
{
  public Task SendToPlayer(string playerGuid, string gameId, Message message);
  public Task SendToAllPlayers(string gameId, Message message);
  public Task EndConnections(string gameId);
  public Task EndConnection(string gameId, string playerGuid);
}
