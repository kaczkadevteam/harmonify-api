using Harmonify.Messages;

namespace Harmonify.Services;

public interface IGameInterruptionService
{
  bool TryStartGame(
    string id,
    StartGameDto data,
    out long timestamp,
    out string uri,
    out string preview_url
  );
  public Task ResumeGame(string gameId, string hostGuid);
  public Task PauseGame(string gameId, string hostGuid);

  public Task<bool> TryEndRoundIfAllGuessessSubmitted(string gameId);
}
