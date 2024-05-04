namespace Harmonify.Messages;

public class MessageErrorWithData<T> : MessageError
{
  public required T Data { get; set; }
}
