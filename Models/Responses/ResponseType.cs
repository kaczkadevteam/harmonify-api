namespace Harmonify.Responses
{
  public enum ResponseType
  {
    Acknowledged,
    CreatedRoom,
    NewPlayer,
    Reconnected,
    ConnectionClosed,
    EndGame,
    ConnectionsList,

    //------ERRORS---------
    NoPlayerInGame,
    IncorrectFormat,
    UnknownError,
  }
}
