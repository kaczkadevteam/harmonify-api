using System.Net.WebSockets;

namespace Harmonify.Models
{
  public class WebSocketConnection
  {
    public required WebSocket WS { get; set; }
    public required string RoomId { get; set; }
    public required string PlayerGuid { get; set; }

    public override string ToString()
    {
      return "Websocket: " + WS + "\nroomId: " + RoomId + "\nguid: " + PlayerGuid;
    }
  }
}
