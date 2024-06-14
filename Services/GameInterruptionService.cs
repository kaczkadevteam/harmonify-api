using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class GameInterruptionService(
  IGameRepository gameRepository,
  IRoundService gameFlowService,
  IWebSocketSenderService webSocketSender
) : IGameInterruptionService
{
  public bool TryStartGame(
    string id,
    StartGameDto data,
    out long timestamp,
    out string uri,
    out string preview_url
  )
  {
    var game = gameRepository.GetGame(id);

    if (game?.State != GameState.GameSetup)
    {
      timestamp = 0;
      uri = "";
      preview_url = "";
      return false;
    }

    game.Tracks = data.Tracks;
    game.DrawnTracks = GameHelper.DrawTracksRandomly(data.Tracks, data.GameSettings.RoundCount);
    game.Settings = data.GameSettings;

    game.CurrentRound = 1;

    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    uri = game.CurrentTrack.Uri;
    preview_url = game.CurrentTrack.Preview_url;
    gameFlowService.StartRound(game, timestamp);
    return true;
  }

  public async Task ResumeGame(string gameId, string hostGuid)
  {
    var game = gameRepository.GetGame(gameId);
    if (game == null || game.Host.Guid != hostGuid || !game.IsPaused)
    {
      return;
    }

    game.IsPaused = false;

    var response = new Message { Type = MessageType.GameResumed };
    await webSocketSender.SendToAllPlayers(gameId, response);

    if (game.State == GameState.RoundResult)
    {
      _ = gameFlowService.WaitAndStartNextRound(game);
    }
  }

  public async Task PauseGame(string gameId, string hostGuid)
  {
    var game = gameRepository.GetGame(gameId);
    if (game == null || game.Host.Guid != hostGuid || game.IsPaused)
    {
      return;
    }

    game.IsPaused = true;
    game.LastPauseDate = DateTime.Now;

    var response = new Message { Type = MessageType.GamePaused };
    await webSocketSender.SendToAllPlayers(gameId, response);
  }

  public async Task<bool> TryEndRoundIfAllGuessessSubmitted(string gameId)
  {
    var game = gameRepository.GetGame(gameId);

    if (game?.State == GameState.RoundPlaying && GameHelper.HasEveryPlayerFinished(game))
    {
      await gameFlowService.EndRound(game);
      return true;
    }

    return false;
  }
}
