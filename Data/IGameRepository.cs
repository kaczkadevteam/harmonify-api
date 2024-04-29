using Harmonify.Models;

namespace Harmonify.Data
{
  public interface IGameRepository
  {
    Game Create(Player host);
    List<Game> GetGames();
    Game? GetGame(string id);
  }
}
