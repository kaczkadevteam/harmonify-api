namespace Harmonify.Responses;

public enum ResponseType
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
