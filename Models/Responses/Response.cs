namespace Harmonify.Responses;

public class Response<T>
{
  public required ResponseType Type { get; set; }
  public T? Data { get; set; }
}
