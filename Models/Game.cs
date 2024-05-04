namespace Harmonify.Models;

public class Game
{
  public required string Id { get; set; }
  public required Player Host { get; set; }
  public required List<Player> Players { get; set; }
  public GameState State { get; set; } = GameState.GameSetup;
}
