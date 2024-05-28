namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  EndGame,
  CloseConnection,
  Guess,
  ChangeName,
  PauseGame,
  ResumeGame,

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
  GamePaused,
  GameResumed,

  //------ERRORS---------
  NoPlayerInGame,
  GameDoesntExist,
  Forbidden,
  Conflict,
  IncorrectFormat,
  UnknownError,
  DataTooLarge,
}
