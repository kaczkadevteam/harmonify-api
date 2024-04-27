namespace Harmonify.Models
{
  public class Game
  {
    public required string RoomId { get; set; }
    public required Player Host { get; set; }
  }
}
