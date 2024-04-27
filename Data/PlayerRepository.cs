using Harmonify.Models;

namespace Harmonify.Data
{
  public class PlayerRepository : IPlayerRepository
  {
    public Player Create()
    {
      return new Player { Guid = Guid.NewGuid().ToString() };
    }
  }
}
