namespace Harmonify.Models;

public class GameStartedDto
{
  public required List<Track> Tracks { get; set; }
  public required List<GameSettings> GameSettings { get; set; }
}
