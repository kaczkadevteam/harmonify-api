using Harmonify.Models;

namespace Harmonify.Messages;

public class GameStartedDto
{
  public required List<DisplayedGuessDto> PossibleGuesses { get; set; }
  public required GameSettings GameSettings { get; set; }
  public required long RoundStartTimestamp { get; init; }
  public required int TrackStart_ms { get; init; }
  public required string Uri { get; init; }
}
