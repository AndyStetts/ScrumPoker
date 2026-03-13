namespace ScrumPoker.Models;

public enum DeckType { Fibonacci, TShirt, PowersOfTwo }

public class CardDeck
{
    public DeckType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Cards { get; set; } = [];

    public static CardDeck Fibonacci() => new()
    {
        Type = DeckType.Fibonacci,
        Name = "Fibonacci",
        Cards = ["0", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "?", "☕"]
    };

    public static CardDeck TShirt() => new()
    {
        Type = DeckType.TShirt,
        Name = "T-Shirt Sizes",
        Cards = ["XS", "S", "M", "L", "XL", "XXL", "?", "☕"]
    };

    public static CardDeck PowersOfTwo() => new()
    {
        Type = DeckType.PowersOfTwo,
        Name = "Powers of 2",
        Cards = ["1", "2", "4", "8", "16", "32", "64", "?", "☕"]
    };

    public static CardDeck FromDeckType(DeckType type) => type switch
    {
        DeckType.TShirt => TShirt(),
        DeckType.PowersOfTwo => PowersOfTwo(),
        _ => Fibonacci()
    };
}
