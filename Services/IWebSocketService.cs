using System.Net.WebSockets;
using Harmonify.Models;

namespace Harmonify.Services
{
  public interface IWebSocketService
  {
    public Task StartConnection(WebSocket webSocket, Game game);
    public string GetWsList();
  }
}
