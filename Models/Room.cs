namespace ScrumPoker.Models;

public class Room
{
    public string Code { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public List<Participant> Participants { get; set; } = [];
    public List<Story> Stories { get; set; } = [];
    public string? CurrentStoryId { get; set; }
    public CardDeck Deck { get; set; } = CardDeck.Fibonacci();
    public bool VotesRevealed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Story? CurrentStory => Stories.FirstOrDefault(s => s.Id == CurrentStoryId);
    public bool AllVoted => Participants.Count > 0 && Participants.All(p => p.HasVoted);
}
