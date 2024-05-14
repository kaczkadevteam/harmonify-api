using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public interface IGameService
{
  public Game Create(Player host);
  public bool GameExists(string id);
  public void AddPlayer(string id, Player player);
  public bool TryChangeName(string id, string playerGuid, string newNickname);
  public bool IsAuthorized(string gameId, string playerGuid, MessageType messageType);
  public void HandlePlayerReconnect(string playerGuid, string gameId);
  public bool TryStartGame(
    string id,
    StartGameDto data,
    out long timestamp,
    out string uri,
    out int trackStart_ms
  );
  public Task<bool> TryEndRoundIfAllGuessessSubmitted(string gameId);
  public Task EndGame(string id);
  public bool TryEvaluatePlayerGuess(string gameId, string playerGuid, string userGuess);
}
