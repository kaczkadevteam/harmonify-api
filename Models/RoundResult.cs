namespace Harmonify.Models;

public class RoundResult
{
  public required int Score { get; set; }
  public required string Guess { get; set; }
  public required GuessLevel GuessLevel { get; set; }
}
