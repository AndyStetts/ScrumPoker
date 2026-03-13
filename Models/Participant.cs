namespace ScrumPoker.Models;

public class Participant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? CurrentVote { get; set; }
    public bool HasVoted => CurrentVote != null;
    public bool IsHost { get; set; }
}
