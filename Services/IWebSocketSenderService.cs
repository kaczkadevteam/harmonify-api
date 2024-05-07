using Harmonify.Messages;

namespace Harmonify.Services;

public interface IWebSocketSenderService
{
  public Task SendToAllPlayers(string gameId, Message message);
}
