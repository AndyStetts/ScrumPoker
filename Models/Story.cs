namespace ScrumPoker.Models;

public class Story
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> Votes { get; set; } = [];
    public string? FinalEstimate { get; set; }
    public bool IsComplete { get; set; }
}
