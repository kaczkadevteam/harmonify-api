using System.Diagnostics.CodeAnalysis;

namespace Harmonify.Models
{
  public class Player
  {
    public required string Guid { get; init; }
    public int Score { get; set; } = 0;

    [SetsRequiredMembers]
    public Player()
    {
      Guid = System.Guid.NewGuid().ToString();
    }
  }
}
