namespace Harmonify.Models;

public class Game
{
  public required string Id { get; set; }
  public required Player Host { get; set; }
  public required List<Player> Players { get; set; }
  public GameState State { get; set; } = GameState.GameSetup;
  public int CurrentRound { get; set; } = 1;
  public List<Track> Tracks { get; set; } = [];
  public GameSettings Settings { get; set; } =
    new GameSettings
    {
      RoundDuration = 30,
      BreakDuration = 3,
      RoundCount = 20,
      TrackDuration = 10,
      TrackStartLowerBound = 0.1f,
      TrackStartUpperBound = 0.9f
    };
}
