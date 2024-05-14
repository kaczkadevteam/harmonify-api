namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  EndGame,
  CloseConnection,
  Guess,
  ChangeName,

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
  EndGameResults,
  NameChanged,
  PlayerList,

  //------ERRORS---------
  NoPlayerInGame,
  GameDoesntExist,
  Forbidden,
  Conflict,
  IncorrectFormat,
  UnknownError,
  DataTooLarge,
}
