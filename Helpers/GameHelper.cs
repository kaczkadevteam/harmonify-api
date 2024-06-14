using Harmonify.Models;

namespace Harmonify.Helpers;

public static class GameHelper
{
  public static bool HasEveryPlayerFinished(Game game)
  {
    return !game.Players.Exists((p) => p.RoundResults.Count != game.CurrentRound);
  }

  public static List<Track> DrawTracksRandomly(List<Track> tracks, int count)
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

  public static void AssertPlayerHasAllRoundResults(Player player, int currentRound)
  {
    while (player.RoundResults.Count < currentRound)
    {
      player.RoundResults.Add(
        new RoundResult
        {
          Guess = "",
          Score = 0,
          GuessLevel = GuessLevel.None
        }
      );
    }
  }

  public static RoundResult GetRoundResult(long guessTime, string userGuess, string correctGuess)
  {
    // See file "punctation_function.png" for visual representation
    int score = (int)
      MathF.Floor(
        guessTime switch
        {
          < 3 => -5 * (guessTime - 3) + 150,
          >= 3 => (100 / MathF.Pow(guessTime - 2, 0.05f)) + 50
        }
      );

    (score, var guessLeel) = userGuess switch
    {
      var g when g == correctGuess => (score, GuessLevel.Full),
      // Guessed album
      var g
        when g.Split(" - ").ElementAtOrDefault(2) == correctGuess.Split(" - ").ElementAtOrDefault(2)
        => (score / 4, GuessLevel.Album),
      // Guessed artist
      var g
        when g.Split(" - ").ElementAtOrDefault(1) == correctGuess.Split(" - ").ElementAtOrDefault(1)
        => (score / 5, GuessLevel.Artist),
      _ => (0, GuessLevel.None)
    };

    return new RoundResult
    {
      Guess = userGuess,
      Score = score,
      GuessLevel = guessLeel
    };
  }
}
