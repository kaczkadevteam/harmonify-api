using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harmonify.Helpers;

public static class JsonHelper
{
  public static readonly JsonStringEnumConverter enumConverter = new(JsonNamingPolicy.CamelCase);

  public static readonly JsonSerializerOptions jsonOptions =
    new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      Converters = { enumConverter },
      TypeInfoResolver = new PolymorphicTypeResolver()
    };
}
