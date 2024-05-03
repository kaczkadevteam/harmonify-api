namespace Harmonify.Messages;

public enum MessageType
{
  //--------INCOMING-----------
  StartGame,
  CloseConnection,
  EndGame,

  //--------OUTGOING-----------
  GameStarted,
  Acknowledged,
  CreatedGame,
  NewPlayer,
  Reconnected,
  ConnectionsList,

  //------ERRORS---------
  NoPlayerInGame,
  GameDoesntExist,
  Forbidden,
  IncorrectStageForMessage,
  IncorrectFormat,
  UnknownError,
}
