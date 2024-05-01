using Harmonify.Models;

namespace Harmonify.Data
{
  public interface IGameRepository
  {
    void Add(Game game);
    List<Game> GetGames();
    bool GameExists(string id);
    Game? GetGame(string id);
    void RemoveGame(string id);
  }
}
