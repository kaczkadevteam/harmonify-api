using Harmonify.Models;

namespace Harmonify.Messages;

//TODO make proper properties for this model (not in scope of task dev-74)
public class EndGameResultsDto
{
  public required Track Track { get; set; }
  public required RoundResult RoundResult { get; set; }
  public required int Score { get; set; }
  public required List<PlayerDto> Players { get; set; }
}
