namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  StartRound,
  EndGame,
  CloseConnection,
  Guess,

  //--------OUTGOING-----------
  CreatedGame,
  NewPlayer,
  GameStarted,
  NextRound,
  RoundStarted,
  RoundFinished,
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
  DataTooLarge,
}
