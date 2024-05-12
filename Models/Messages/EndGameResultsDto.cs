using Harmonify.Models;

namespace Harmonify.Messages;

public class EndGameResultsDto
{
  public required List<Track> Tracks { get; set; }
  public required List<RoundResult> RoundResults { get; set; }
  public required int Score { get; set; }
  public required List<PlayerDto> Players { get; set; }
}
