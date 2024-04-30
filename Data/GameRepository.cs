using Harmonify.Models;

namespace Harmonify.Data
{
  public class GameRepository : IGameRepository
  {
    private readonly List<Game> games = [];

    private const int minGameIdNumber = 1000;
    private const int maxGameIdNumber = 10_000;
    private int nextGameId = 1000;

    public Game Create(Player host)
    {
      var game = new Game
      {
        Host = host,
        Id = nextGameId.ToString(),
        Players = new List<Player>()
      };
      games.Add(game);

      if (nextGameId >= maxGameIdNumber)
      {
        nextGameId = minGameIdNumber;
      }
      else
      {
        nextGameId++;
      }

      return game;
    }

    public List<Game> GetGames()
    {
      return games;
    }

    public Game? GetGame(string id)
    {
      return games.Find((game) => game.Id == id);
    }

    public void RemoveGame(string id)
    {
      Console.WriteLine($"Removed game {id}");
      games.RemoveAll((game) => game.Id == id);
    }
  }
}
