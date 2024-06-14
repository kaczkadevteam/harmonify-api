using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class PlayerService(IGameRepository gameRepository) : IPlayerService
{
  public void AddPlayer(string id, Player player)
  {
    var game = gameRepository.GetGame(id);
    if (game == null)
    {
      return;
    }

    while (game.Players.Any(p => p.Nickname == player.Nickname))
    {
      player.Nickname = NameGenerator.GetName();
    }
    game.Players.Add(player);
  }

  public bool TryChangeName(string id, string playerGuid, string newNickname)
  {
    var game = gameRepository.GetGame(id);
    if (game?.State != GameState.GameSetup)
    {
      return false;
    }
    var player = game.Players.Find(player => player.Guid == playerGuid);
    var nicknameAlreadyUsed = game.Players.Any(player => player.Nickname == newNickname);
    if (player == null || nicknameAlreadyUsed)
    {
      return false;
    }
    player.Nickname = newNickname;
    return true;
  }

  public bool TryEvaluatePlayerGuess(string gameId, string playerGuid, string userGuess)
  {
    var game = gameRepository.GetGame(gameId);
    var player = game?.Players.Find((p) => p.Guid == playerGuid);

    if (
      game?.State != GameState.RoundPlaying
      || player == null
      || player.RoundResults.Count == game.CurrentRound
    )
    {
      return false;
    }

    var guessTime =
      (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - game.RoundStartTimestamp) / 1000;
    var correctGuess = game.CurrentTrack.Guess;

    var roundResult = GameHelper.GetRoundResult(guessTime, userGuess, correctGuess);

    player.Score += roundResult.Score;
    player.RoundResults.Add(roundResult);

    return true;
  }

  public bool IsAuthorized(string gameId, string playerGuid, MessageType messageType)
  {
    if (messageType == MessageType.StartGame || messageType == MessageType.EndGame)
    {
      return gameRepository.GetGame(gameId)?.Host.Guid == playerGuid;
    }

    return true;
  }

  public void HandlePlayerReconnect(string playerGuid, string gameId)
  {
    throw new NotImplementedException();
  }
}
