namespace Harmonify.Messages;

public class MessageError : Message
{
  public required string ErrorMessage { get; set; }
}
