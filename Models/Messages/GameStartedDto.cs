using Harmonify.Models;

namespace Harmonify.Messages;

public class GameStartedDto
{
  public required List<DisplayedGuessDto> PossibleGuesses { get; set; }
  public required GameSettings GameSettings { get; set; }
}
