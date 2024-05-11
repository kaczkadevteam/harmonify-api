using Harmonify.Data;
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

  public bool TryStartGame(string id, StartGameDto data)
  {
    var game = gameRepository.GetGame(id);

    if (game?.State != GameState.GameSetup)
    {
      return false;
    }

    game.Tracks = data.Tracks;
    game.DrawnTracks = DrawTracksRandomly(data.Tracks, data.GameSettings.RoundCount);
    game.Settings = data.GameSettings;
    game.CurrentRound = 1;
    game.State = GameState.RoundSetup;
    return true;
  }

  public bool TryStartRound(string id)
  {
    var game = gameRepository.GetGame(id);

    if (game?.State != GameState.RoundSetup)
    {
      return false;
    }

    game.State = GameState.RoundPlaying;
    game.RoundStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    Task.Run(async () =>
    {
      await Task.Delay(TimeSpan.FromSeconds(game.Settings.RoundDuration));

      await EndRound(game);
    });
    return true;
  }

  public void AddPlayer(string id, Player player)
  {
    gameRepository.GetGame(id)?.Players.Add(player);
  }

  public void HandlePlayerReconnect(string playerGuid, string gameId)
  {
    throw new NotImplementedException();
  }

  public async Task EndGame(string id)
  {
    await webSocketSender.EndConnections(id);
    gameRepository.RemoveGame(id);
  }

  public bool IsAuthorized(string gameId, string playerGuid, MessageType messageType)
  {
    if (
      messageType == MessageType.StartGame
      || messageType == MessageType.EndGame
      || messageType == MessageType.StartRound
    )
    {
      return gameRepository.GetGame(gameId)?.Host.Guid == playerGuid;
    }

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

    // See file "punctation_function.png" for visual representation
    int score = (int)
      MathF.Floor(
        guessTime switch
        {
          < 3 => -5 * (guessTime - 3) + 150,
          >= 3 => (100 / MathF.Pow(guessTime - 2, 0.05f)) + 50
        }
      );

    var trackGuess = game.DrawnTracks[game.CurrentRound - 1].Guess;
    score = userGuess switch
    {
      var g when g == trackGuess => score,
      // Guessed album
      var g when g.Split(" - ")[2] == trackGuess.Split(" - ")[2] => score / 4,
      // Guessed artist
      var g when g.Split(" - ")[1] == trackGuess.Split(" - ")[1] => score / 5,
      _ => 0
    };

    player.Score += score;
    player.RoundResults.Add(new RoundResult { Guess = userGuess, Score = score });

    return true;
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

  private async Task EndRound(Game game)
  {
    if (game.CurrentRound == game.Settings.RoundCount)
    {
      game.State = GameState.GameResult;
      await EndGame(game.Id);
      return;
    }

    game.CurrentRound++;
    game.State = GameState.RoundSetup;

    var response = new MessageWithData<int>
    {
      Type = MessageType.NextRound,
      Data = game.CurrentRound
    };
    await webSocketSender.SendToAllPlayers(game.Id, response);
  }

  private bool HasEveryPlayerFinished(Game game)
  {
    return !game.Players.Exists((p) => p.RoundResults.Count != game.CurrentRound);
  }
}
