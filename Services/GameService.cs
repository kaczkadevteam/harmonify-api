using Harmonify.Data;
using Harmonify.Messages;
using Harmonify.Models;

namespace Harmonify.Services;

public class GameService(
  IGameRepository gameRepository /*,IWebSocketService webSocketService*/
) : IGameService
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

  public bool TryStartGame(string id)
  {
    var game = gameRepository.GetGame(id);

    if (game?.State != GameState.GameSetup)
    {
      return false;
    }

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
    Task.Run(async () =>
    {
      //TODO: Use time given by host
      await Task.Delay(TimeSpan.FromSeconds(15));

      game.CurrentRound++;
      game.State = GameState.RoundSetup;
      //TODO: Remove circular dependency
      /*var response = new MessageWithData<int>
      {
        Type = MessageType.NextRound,
        Data = game.CurrentRound
      };
      await webSocketService.SendToAllPlayers(id, response);*/

      Console.WriteLine($"Next Round: {game.CurrentRound}");
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

  public void RemoveGame(string id)
  {
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
}
