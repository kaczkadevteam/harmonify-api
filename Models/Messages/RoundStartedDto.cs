namespace Harmonify.Messages;

public class RoundStartedDto
{
  public required int RoundNumber { get; init; }
  public required long RoundStartTimestamp { get; set; }
  public required int TrackStart_ms { get; init; }
  public required String Uri { get; init; }
}
