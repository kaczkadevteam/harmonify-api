namespace Harmonify.Messages;

public class MessageWithData<T> : Message
{
  public required T Data { get; set; }
}
