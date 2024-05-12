using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Harmonify.Messages;

namespace Harmonify.Helpers;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
  public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
  {
    JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

    Type basePointType = typeof(Message);
    if (jsonTypeInfo.Type == basePointType)
    {
      jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
      {
        TypeDiscriminatorPropertyName = "$type",
        IgnoreUnrecognizedTypeDiscriminators = true,
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        DerivedTypes =
        {
          new JsonDerivedType(typeof(Message), "message"),
          new JsonDerivedType(typeof(MessageError), "messageError"),
          new JsonDerivedType(typeof(MessageWithData<CreatedGameDto>), "message/createdGameDto"),
          new JsonDerivedType(typeof(MessageWithData<StartGameDto>), "message/startGameDto"),
          new JsonDerivedType(typeof(MessageWithData<GameStartedDto>), "message/gameStartedDto"),
          new JsonDerivedType(typeof(MessageWithData<RoundStartedDto>), "message/roundStartedDto"),
          new JsonDerivedType(
            typeof(MessageWithData<RoundFinishedDto>),
            "message/roundFinishedDto"
          ),
          new JsonDerivedType(
            typeof(MessageWithData<EndGameResultsDto>),
            "message/endGameResultsDto"
          ),
          new JsonDerivedType(typeof(MessageWithData<string>), "message/string"),
          new JsonDerivedType(typeof(MessageWithData<int>), "message/int"),
          new JsonDerivedType(typeof(MessageWithData<long>), "message/long"),
        }
      };
    }

    return jsonTypeInfo;
  }
}
