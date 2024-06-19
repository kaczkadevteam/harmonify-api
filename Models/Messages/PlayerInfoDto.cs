namespace Harmonify.Models;

public class PlayerInfoDto
{
  public required string Guid { get; init; }
  public required string Nickname { get; set; }
  public required bool Connected { get; set; }
}
