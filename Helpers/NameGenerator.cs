namespace Harmonify.Helpers;

public class NameGenerator
{
  static readonly string[] adjectives =
  [
    "Happy",
    "Joyful",
    "Laughing",
    "Smiling",
    "Clever",
    "Brave",
    "Silly",
    "Wise",
    "Cunning",
    "Gentle",
    "Swift",
    "Bright",
    "Mystic",
    "Vivid",
    "Calm",
    "Fierce",
    "Radiant",
    "Vibrant",
    "Serene",
    "Epic"
  ];

  static readonly string[] colors =
  [
    "Red",
    "Blue",
    "Green",
    "Yellow",
    "Orange",
    "Purple",
    "Pink",
    "Turquoise",
    "Gold",
    "Silver",
    "Crimson",
    "Indigo",
    "Emerald",
    "Ruby",
    "Sapphire",
    "Amber",
    "Azure",
    "Magenta",
    "Cerulean",
    "Violet"
  ];

  static readonly string[] animals =
  [
    "Fox",
    "Wolf",
    "Bear",
    "Lion",
    "Tiger",
    "Eagle",
    "Dragon",
    "Serpent",
    "Raven",
    "Phoenix",
    "Panther",
    "Leopard",
    "Owl",
    "Falcon",
    "Hawk",
    "Badger",
    "Coyote",
    "Jaguar",
    "Griffin",
    "Unicorn"
  ];

  public static string GetName()
  {
    string name =
      adjectives[Random.Shared.Next(adjectives.Length)]
      + colors[Random.Shared.Next(colors.Length)]
      + animals[Random.Shared.Next(animals.Length)]
      + Random.Shared.Next(100);
    return name;
  }
}
