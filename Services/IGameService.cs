using Harmonify.Models;

namespace Harmonify.Services
{
  public interface IGameService
  {
    public Player AddNewPlayer(Game game);
    public void RemoveGame(string id);
  }
}
