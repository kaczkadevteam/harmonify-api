namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  StartRound,
  EndGame,
  CloseConnection,

  //--------OUTGOING-----------
  CreatedGame,
  NewPlayer,
  GameStarted,
  NextRound,
  RoundStarted,
  Acknowledged,
  Reconnected,
  ConnectionsList,
  DataTooLarge,

  //------ERRORS---------
  NoPlayerInGame,
  GameDoesntExist,
  Forbidden,
  Conflict,
  IncorrectFormat,
  UnknownError,
}
