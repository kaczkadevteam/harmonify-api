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

  public async Task QuitGame(string gameId, string playerGuid)
  {
    var game = gameRepository.GetGame(gameId);
    if (game == null)
    {
      return;
    }
    var player = game.Players.Find(player => player.Guid == playerGuid);
    if (player == null)
    {
      return;
    }

    if (player.Guid == game.Host.Guid && game.State == GameState.GameSetup)
    {
      await RemoveGameAndConnections(game.Id);
      return;
    }

    if (game.State == GameState.RoundPlaying || game.State == GameState.RoundResult)
    {
      await SendGameResultToPlayers(game, [player]);
    }

    game.Players.Remove(player);

    await SendPlayerList(game);
  }

  public async Task EndGame(string id)
  {
    var game = gameRepository.GetGame(id);

    if (game == null)
    {
      return;
    }

    await SendGameResultToPlayers(game, game.Players);

    game.State = GameState.GameResult;
    await RemoveGameAndConnections(game.Id);
  }

  public async Task RemoveGameAndConnections(string gameId)
  {
    await webSocketSender.EndConnections(gameId);
    gameRepository.RemoveGame(gameId);
  }

  public async Task SendPlayerList(string gameId)
  {
    var game = gameRepository.GetGame(gameId);
    if (game == null)
    {
      return;
    }
    await SendPlayerList(game);
  }

  private async Task SendPlayerList(Game game)
  {
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
    await webSocketSender.SendToAllPlayers(game.Id, response);
  }

  private async Task SendGameResultToPlayers(Game game, IList<Player> players)
  {
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

    await Task.WhenAll(
      players.Select(
        async (player) =>
        {
          var response = new MessageWithData<EndGameResultsDto>
          {
            Type = MessageType.EndGameResults,
            Data = new EndGameResultsDto { Tracks = game.DrawnTracks, Players = playersDto }
          };

          await webSocketSender.SendToPlayer(player.Guid, game.Id, response);
        }
      )
    );
  }
}
