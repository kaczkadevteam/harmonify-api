using System.Net.WebSockets;

namespace Harmonify.Models;

public class WebSocketConnection
{
  public required WebSocket WS { get; set; }

  public byte[] Buffer { get; set; } = new byte[1024 * 8];
  public required string GameId { get; set; }
  public required string PlayerGuid { get; set; }

  public override string ToString()
  {
    return $@"Websocket: {WS} 
gameId: {GameId}
guid: {PlayerGuid}
socket state: {WS.State}";
  }
}
