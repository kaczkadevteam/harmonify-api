using System.Net.WebSockets;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public interface IWebSocketSenderService
{
  public Task SendToAllPlayers(string gameId, Message message);
  public Task EndConnections(string gameId);
}
