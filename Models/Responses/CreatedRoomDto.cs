namespace Harmonify.Messages;

public class CreatedGameDto
{
  public required string GameId { get; init; }
  public required string HostGuid { get; init; }
}
