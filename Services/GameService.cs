using Harmonify.Data;
using Harmonify.Helpers;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class GameService(IGameRepository gameRepository, IWebSocketSenderService webSocketSender)
  : IGameService
{
  private const int minGameIdNumber = 1000;
  private const int maxGameIdNumber = 10_000;
  private int nextGameId = 1000;

  public Game Create(Player host)
  {
    var game = new Game
    {
      Host = host,
      Id = nextGameId.ToString(),
      Players = [host]
    };
    gameRepository.Add(game);

    if (nextGameId >= maxGameIdNumber)
    {
      nextGameId = minGameIdNumber;
    }
    else
    {
      nextGameId++;
    }

    return game;
  }

  public bool GameExists(string id)
  {
    return gameRepository.GameExists(id);
  }

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

  public async Task SendPlayerList(string gameId)
  {
    var game = gameRepository.GetGame(gameId);
    if (game == null)
    {
      return;
    }
    var response = new MessageWithData<List<PlayerInfoDto>>
    {
      Type = MessageType.PlayerList,
      Data = game
        .Players.Select(player => new PlayerInfoDto
        {
          Guid = player.Guid,
          Nickname = player.Nickname
        })
        .ToList()
    };
    await webSocketSender.SendToAllPlayers(gameId, response);
  }

  public bool TryStartGame(
    string id,
    StartGameDto data,
    out long timestamp,
    out string uri,
    out int trackStart_ms
  )
  {
    var game = gameRepository.GetGame(id);

    if (game?.State != GameState.GameSetup)
    {
      timestamp = 0;
      uri = "";
      trackStart_ms = 0;
      return false;
    }

    game.Tracks = data.Tracks;
    game.DrawnTracks = DrawTracksRandomly(data.Tracks, data.GameSettings.RoundCount);
    game.Settings = data.GameSettings;

    game.CurrentRound = 1;

    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    uri = game.CurrentTrack.Uri;
    trackStart_ms = GetTrackStart(game.Settings, game.CurrentTrack);
    StartRound(game, timestamp);
    return true;
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
        TrackStart_ms = GetTrackStart(game.Settings, game.CurrentTrack)
      }
    };

    StartRound(game, timestamp);
    await webSocketSender.SendToAllPlayers(game.Id, response);
  }

  private void StartRound(Game game, long timestamp)
  {
    game.State = GameState.RoundPlaying;
    game.RoundStartTimestamp = timestamp;
    var roundToEnd = game.CurrentRound;

    Task.Run(async () =>
    {
      await Task.Delay(TimeSpan.FromSeconds(game.Settings.RoundDuration));
      if (roundToEnd != game.CurrentRound)
      {
        return;
      }
      await EndRound(game);
    });
  }

  public async Task<bool> TryEndRoundIfAllGuessessSubmitted(string gameId)
  {
    var game = gameRepository.GetGame(gameId);

    if (game?.State == GameState.RoundPlaying && HasEveryPlayerFinished(game))
    {
      await EndRound(game);
      return true;
    }

    return false;
  }

  private async Task EndRound(Game game)
  {
    if (game.State != GameState.RoundPlaying)
    {
      return;
    }

    if (game.CurrentRound == game.Settings.RoundCount)
    {
      game.State = GameState.GameFinish;
      await EndGame(game.Id);
      return;
    }

    game.State = GameState.RoundFinish;

    var playersDto = game
      .Players.Select(
        (player) =>
          new PlayerDto
          {
            Guid = player.Guid,
            Nickname = player.Nickname,
            Score = player.Score
          }
      )
      .ToList();

    await Task.WhenAll(
      game.Players.Select(async (player) => await SendPlayerRoundResult(game, player, playersDto))
    );

    await Task.Delay(TimeSpan.FromSeconds(game.Settings.BreakDurationBetweenRounds));
    await StartNextRound(game);
  }

  private async Task SendPlayerRoundResult(Game game, Player player, List<PlayerDto> playersDto)
  {
    if (player.RoundResults.Count != game.CurrentRound)
    {
      player.RoundResults.Add(new RoundResult { Guess = "", Score = 0 });
    }

    var response = new MessageWithData<RoundFinishedDto>
    {
      Type = MessageType.NextRound,
      Data = new RoundFinishedDto
      {
        Track = game.CurrentTrack,
        Score = player.Score,
        RoundResult = player.RoundResults.Last(),
        Players = playersDto
      }
    };

    await webSocketSender.SendToPlayer(player.Guid, game.Id, response);
  }

  public async Task EndGame(string id)
  {
    var game = gameRepository.GetGame(id);

    if (game == null)
    {
      return;
    }

    game.State = GameState.GameFinish;

    var playersDto = game
      .Players.Select(
        (player) =>
          new PlayerDto
          {
            Guid = player.Guid,
            Nickname = player.Nickname,
            Score = player.Score
          }
      )
      .ToList();

    await Task.WhenAll(
      game.Players.Select(
        async (player) =>
        {
          if (player.RoundResults.Count != game.CurrentRound)
          {
            player.RoundResults.Add(new RoundResult { Guess = "", Score = 0 });
          }

          var response = new MessageWithData<EndGameResultsDto>
          {
            Type = MessageType.EndGameResults,
            Data = new EndGameResultsDto
            {
              Tracks = game.DrawnTracks,
              Score = player.Score,
              RoundResults = player.RoundResults,
              Players = playersDto
            }
          };

          await webSocketSender.SendToPlayer(player.Guid, game.Id, response);
        }
      )
    );
    await webSocketSender.EndConnections(id);
    gameRepository.RemoveGame(id);
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

    // See file "punctation_function.png" for visual representation
    int score = (int)
      MathF.Floor(
        guessTime switch
        {
          < 3 => -5 * (guessTime - 3) + 150,
          >= 3 => (100 / MathF.Pow(guessTime - 2, 0.05f)) + 50
        }
      );

    var trackGuess = game.CurrentTrack.Guess;
    score = userGuess switch
    {
      var g when g == trackGuess => score,
      // Guessed album
      var g
        when g.Split(" - ").ElementAtOrDefault(2) == trackGuess.Split(" - ").ElementAtOrDefault(2)
        => score / 4,
      // Guessed artist
      var g
        when g.Split(" - ").ElementAtOrDefault(1) == trackGuess.Split(" - ").ElementAtOrDefault(1)
        => score / 5,
      _ => 0
    };

    player.Score += score;
    player.RoundResults.Add(new RoundResult { Guess = userGuess, Score = score });

    return true;
  }

  private static bool HasEveryPlayerFinished(Game game)
  {
    return !game.Players.Exists((p) => p.RoundResults.Count != game.CurrentRound);
  }

  private static List<Track> DrawTracksRandomly(List<Track> tracks, int count)
  {
    List<Track> drawnTracks = [];
    var leftTracks = tracks[..];

    for (var i = 0; i < count; i++)
    {
      if (leftTracks.Count == 0)
      {
        leftTracks = tracks[..];
      }

      var drawnIndex = Random.Shared.Next(leftTracks.Count);
      drawnTracks.Add(leftTracks[drawnIndex]);
      leftTracks.RemoveAt(drawnIndex);
    }

    return drawnTracks;
  }

  private static int GetTrackStart(GameSettings gameSettings, Track track)
  {
    int lowerLimit = (int)Math.Floor(gameSettings.TrackStartLowerBound * track.Duration_ms);
    int upperLimit = (int)Math.Floor(gameSettings.TrackStartUpperBound * track.Duration_ms);
    int trackstart_ms = Math.Min(
      Random.Shared.Next(lowerLimit, upperLimit),
      track.Duration_ms - gameSettings.TrackDuration * 1000
    );
    return trackstart_ms;
  }
}
