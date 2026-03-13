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
    public string? HostTransferMessage { get; set; }

    public Story? CurrentStory => Stories.FirstOrDefault(s => s.Id == CurrentStoryId);
    public bool AllVoted => Participants.Any() && Participants.Where(p => !p.IsHost).All(p => p.HasVoted);
}
