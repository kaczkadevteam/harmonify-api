namespace Harmonify.Models
{
  public class Player
  {
    private int score = 0;

    public required string Guid { get; init; }
    public int Score
    {
      get { return score; }
    }

    public void AddPoints(int points)
    {
      score += points;
    }

    public void ResetScore()
    {
      score = 0;
    }
  }
}
