using Harmonify.Models;

namespace Harmonify.Services;

public interface IRoundService
{
  void StartRound(Game game, long timestamp);
  Task EndRound(Game game);

  Task WaitAndStartNextRound(Game game);
}
