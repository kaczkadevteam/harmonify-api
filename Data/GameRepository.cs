using Harmonify.Models;

namespace Harmonify.Data
{
  public class GameRepository : IGameRepository
  {
    private readonly List<Game> games = [];

    public void Add(Game game)
    {
      games.Add(game);
    }

    public List<Game> GetGames()
    {
      return games;
    }

    public bool GameExists(string id)
    {
      return games.Exists((g) => g.Id == id);
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
