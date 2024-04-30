using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harmonify.Handlers
{
  public static class JsonHandler
  {
    public static readonly JsonStringEnumConverter enumConverter = new(JsonNamingPolicy.CamelCase);

    public static readonly JsonSerializerOptions jsonOptions =
      new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { enumConverter } };
  }
}
