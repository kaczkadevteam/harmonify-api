namespace Harmonify.Models;

public class Game
{
  public required string Id { get; set; }
  public required Player Host { get; set; }
  public required List<Player> Players { get; set; }
  public GameState State { get; set; } = GameState.GameSetup;
  public int CurrentRound { get; set; } = 1;
  public long RoundStartTimestamp { get; set; } = 0;
  public List<Track> Tracks { get; set; } = [];
  public List<Track> DrawnTracks { get; set; } = [];
  public GameSettings Settings { get; set; } =
    new GameSettings
    {
      RoundDuration = 30,
      BreakDurationBetweenTrackPlays = 3,
      BreakDurationBetweenRounds = 10,
      RoundCount = 20,
      TrackDuration = 10,
      TrackStartLowerBound = 0.1f,
      TrackStartUpperBound = 0.9f
    };
}
