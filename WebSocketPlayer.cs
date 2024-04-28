using System.Net.WebSockets;

public class WebSocketPlayer(WebSocket webSocket, string guid, string roomId)
{
  public WebSocket webSocket = webSocket;
  public string roomId = roomId;
  public string guid = guid;

  public override string ToString()
  {
    return "Websocket: " + webSocket + "\nroomId: " + roomId + "\nguid: " + guid;
  }
}
