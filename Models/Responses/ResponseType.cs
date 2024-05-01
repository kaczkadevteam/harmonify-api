namespace Harmonify.Responses
{
  public enum ResponseType
  {
    Acknowledged,
    CreatedGame,
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
