using Harmonify.Models;

namespace Harmonify.Data
{
  public interface IGameRepository
  {
    Game Create(Player host);
  }
}
