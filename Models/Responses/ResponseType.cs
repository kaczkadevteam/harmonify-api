namespace Harmonify.Responses
{
  public enum ResponseType
  {
    Acknowledged,
    CreatedRoom,
    NewPlayer,
    ConnectionClosed,
    EndGame,
    ConnectionsList,

    //------ERRORS---------
    NoPlayerInGame,
    IncorrectFormat,
    UnknownError,
  }
}
