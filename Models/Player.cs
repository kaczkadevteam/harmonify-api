using System.Diagnostics.CodeAnalysis;
using Harmonify.Helpers;

namespace Harmonify.Models;

public class Player
{
  public required string Nickname { get; set; }
  public required string Guid { get; init; }
  public int Score { get; set; } = 0;
  public List<RoundResult> RoundResults { get; set; } = [];

  [SetsRequiredMembers]
  public Player()
  {
    Guid = System.Guid.NewGuid().ToString();
    Nickname = NameGenerator.GetName();
  }
}

public class RoundResult
{
  public required int Score { get; set; }
  public required string Guess { get; set; }
}
