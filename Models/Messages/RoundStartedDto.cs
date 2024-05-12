namespace Harmonify.Messages;

public class RoundStartedDto
{
  public required int RoundNumber { get; init; }
  public required long RoundStartTimestamp { get; set; }
}
