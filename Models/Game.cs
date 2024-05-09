namespace Harmonify.Models;

public class Game
{
  public required string Id { get; set; }
  public required Player Host { get; set; }
  public required List<Player> Players { get; set; }
  public GameState State { get; set; } = GameState.GameSetup;
  public int CurrentRound { get; set; } = 1;
  public List<Track> Tracks { get; set; } = [];
  public GameSettings Settings { get; set; } = new GameSettings { RoundTime = 10 };
}
