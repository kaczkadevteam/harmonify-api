using System.Net.WebSockets;

public class WebSocketPlayer(WebSocket webSocket, string guid)
{
    public WebSocket webSocket = webSocket;
    public string guid = guid;
}