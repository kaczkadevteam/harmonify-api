using Harmonify.Models;

namespace Harmonify.Data
{
  public class GameRepository : IGameRepository
  {
    private readonly List<Game> games = [];

    private const int minRoomNumber = 1000;
    private const int maxRoomNumber = 10_000;
    private int nextRoom = 1000;

    public Game Create(Player host)
    {
      var game = new Game { Host = host, RoomId = nextRoom.ToString() };
      games.Add(game);

      if (nextRoom == maxRoomNumber)
      {
        nextRoom = minRoomNumber;
      }
      else
      {
        nextRoom++;
      }

      return game;
    }
  }
}
