using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public interface IGameService
{
  public Game Create(Player host);
  public bool GameExists(string id);
  public bool TryStartGame(string id, StartGameDto data);
  public bool TryStartRound(string id);
  public void AddPlayer(string id, Player player);
  public Task EndGame(string id);
  public void HandlePlayerReconnect(string playerGuid, string gameId);
  public bool IsAuthorized(string gameId, string playerGuid, MessageType messageType);
  public bool TryEvaluatePlayerGuess(string gameId, string playerGuid, string userGuess);
  public Task<bool> TryEndRoundIfAllPlayersSubmittedGuess(string gameId);
}
