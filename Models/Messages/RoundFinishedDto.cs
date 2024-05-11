using Harmonify.Models;

namespace Harmonify.Messages;

public class RoundFinishedDto
{
  public required Track Track { get; set; }
  public required string UserGuess { get; set; }
  public required RoundResult RoundResult { get; set; }
  public required int Score { get; set; }
  public required List<PlayerDto> players;
}
