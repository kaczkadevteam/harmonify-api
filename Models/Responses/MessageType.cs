namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  CloseConnection,
  EndGame,

  //--------OUTGOING-----------
  CreatedGame,
  NewPlayer,
  GameStarted,
  NextRound,
  Acknowledged,
  Reconnected,
  ConnectionsList,

  //------ERRORS---------
  NoPlayerInGame,
  GameDoesntExist,
  Forbidden,
  Conflict,
  IncorrectFormat,
  UnknownError,
}
