using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class RoundService(IWebSocketSenderService webSocketSender, IGameService gameService)
  : IRoundService
{
  public void StartRound(Game game, long timestamp)
  {
    game.State = GameState.RoundPlaying;
    game.RoundStartTimestamp = timestamp;

    _ = WaitAndEndRound(game);
  }

  private async Task WaitAndEndRound(Game game)
  {
    await Task.Run(async () =>
    {
      var roundToEnd = game.CurrentRound;

      await Task.Delay(TimeSpan.FromSeconds(game.Settings.RoundDuration));
      if (roundToEnd != game.CurrentRound)
      {
        return;
      }
      await EndRound(game);
    });
  }

  public async Task EndRound(Game game)
  {
    if (game.State != GameState.RoundPlaying)
    {
      return;
    }

    if (game.CurrentRound == game.Settings.RoundCount)
    {
      game.State = GameState.GameResult;
      await gameService.EndGame(game.Id);
      return;
    }
    game.State = GameState.RoundResult;

    game.Players.ForEach(
      (player) => GameHelper.AssertPlayerHasAllRoundResults(player, game.CurrentRound)
    );

    var playersDto = game
      .Players.Select(
        (player) =>
          new PlayerDto
          {
            Guid = player.Guid,
            Nickname = player.Nickname,
            Score = player.Score,
            RoundResults = player.RoundResults
          }
      )
      .ToList();

    var response = new MessageWithData<RoundFinishedDto>
    {
      Type = MessageType.NextRound,
      Data = new RoundFinishedDto { Track = game.CurrentTrack, Players = playersDto }
    };
    await webSocketSender.SendToAllPlayers(game.Id, response);

    _ = WaitAndStartNextRound(game);
  }

  public async Task WaitAndStartNextRound(Game game)
  {
    await Task.Run(async () =>
    {
      if (game.IsPaused)
        return;

      var startDate = DateTime.Now;
      await Task.Delay(TimeSpan.FromSeconds(game.Settings.BreakDurationBetweenRounds));

      if (game.LastPauseDate > startDate)
        return;

      await StartNextRound(game);
    });
  }

  private async Task StartNextRound(Game game)
  {
    game.CurrentRound++;

    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var response = new MessageWithData<RoundStartedDto>
    {
      Type = MessageType.NextRound,
      Data = new RoundStartedDto
      {
        RoundNumber = game.CurrentRound,
        RoundStartTimestamp = timestamp,
        Uri = game.CurrentTrack.Uri,
        TrackStart_ms = 0,
        Preview_url = game.CurrentTrack.Preview_url
      }
    };

    StartRound(game, timestamp);
    await webSocketSender.SendToAllPlayers(game.Id, response);
  }
}
