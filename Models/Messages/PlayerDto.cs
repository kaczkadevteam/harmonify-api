namespace Harmonify.Models;

public class PlayerDto
{
  public required string Guid { get; init; }
  public required string Nickname { get; set; }
  public required int Score { get; set; }
  public required List<RoundResult> RoundResults { get; set; }
}
