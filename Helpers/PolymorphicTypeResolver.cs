using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Harmonify.Messages;

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
          new JsonDerivedType(typeof(MessageErrorWithData<>), "messageErrorWithData"),
          new JsonDerivedType(typeof(MessageWithData<>), "messageWithData"),
        }
      };
    }

    return jsonTypeInfo;
  }
}
