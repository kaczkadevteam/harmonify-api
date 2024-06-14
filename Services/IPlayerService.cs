using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public interface IPlayerService
{
  void AddPlayer(string id, Player player);
  bool TryChangeName(string id, string playerGuid, string newNickname);
  bool TryEvaluatePlayerGuess(string gameId, string playerGuid, string userGuess);
  bool IsAuthorized(string gameId, string playerGuid, MessageType messageType);
  void HandlePlayerReconnect(string playerGuid, string gameId);
}
