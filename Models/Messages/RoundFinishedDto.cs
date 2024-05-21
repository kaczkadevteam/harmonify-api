using Harmonify.Models;

namespace Harmonify.Messages;

public class RoundFinishedDto
{
  public required Track Track { get; set; }
  public required List<PlayerDto> Players { get; set; }
}
