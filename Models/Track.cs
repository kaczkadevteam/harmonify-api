namespace Harmonify.Models;

public class Track
{
  public required List<Artist> Artists { get; set; }
  public required string Duration_ms { get; set; }
  public required string Name { get; set; }
  public required string Uri { get; set; }
  public required string Guess { get; set; }
  public required int TrackStart_ms { get; set; }
}

public class Artist
{
  public required string Name { get; set; }
  public required string Id { get; set; }
}

public class Album
{
  public required string Name { get; set; }
  public required List<Image> Images { get; set; }
}

public class Image
{
  public required string Url { get; set; }
  public int Height { get; set; }
  public int Width { get; set; }
}
