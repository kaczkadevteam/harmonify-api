namespace Harmonify.Models;

public class Track
{
  public required List<Artist> Artists { get; init; }
  public required int Duration_ms { get; init; }
  public required string Name { get; init; }
  public required string Uri { get; init; }
  public required Album Album { get; init; }
  public required string Preview_url { get; init; }
  private string? guess;
  public string Guess
  {
    get
    {
      if (guess == null)
      {
        var artistsString = Artists.Aggregate(
          "",
          (acc, artist) =>
          {
            return $"{acc}, {artist.Name}";
          }
        )[2..];
        guess = $"{Name} - {artistsString} - {Album.Name}";
      }
      return guess;
    }
  }
}

public class Artist
{
  public required string Name { get; init; }
  public required string Id { get; init; }
}

public class Album
{
  public required string Name { get; init; }
  public required List<Image> Images { get; init; }
}

public class Image
{
  public required string Url { get; init; }
  public int Height { get; init; }
  public int Width { get; init; }
}
