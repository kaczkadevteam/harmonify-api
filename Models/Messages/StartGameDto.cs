namespace Harmonify.Messages;

using Harmonify.Models;

public class StartGameDto
{
  public required List<Track> Tracks { get; set; }
  public required GameSettings GameSettings { get; set; }
}
