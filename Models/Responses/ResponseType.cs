using System.Text.Json.Serialization;

namespace Harmonify.Responses
{
  [JsonConverter(typeof(JsonStringEnumConverter))]
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
