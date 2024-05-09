namespace Harmonify.Models;

public class GameSettings
{
  public required int BreakDuration { get; set; }
  public required int TrackDuration { get; set; }
  public required int RoundDuration { get; set; }
  public required int RoundCount { get; set; }
  public required float TrackStartLowerBound { get; set; }
  public required float TrackStartUpperBound { get; set; }
}
