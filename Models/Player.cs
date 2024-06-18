using System.Diagnostics.CodeAnalysis;
using Harmonify.Helpers;

namespace Harmonify.Models;

[method: SetsRequiredMembers]
public class Player()
{
  public required string Nickname { get; set; } = NameGenerator.GetName();
  public required string Guid { get; init; } = System.Guid.NewGuid().ToString();
  public int Score { get; set; } = 0;
  public List<RoundResult> RoundResults { get; set; } = [];
  public bool Connected { get; set; } = true;
}
