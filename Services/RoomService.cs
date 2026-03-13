using System.Collections.Concurrent;
using ScrumPoker.Models;

namespace ScrumPoker.Services;

public class RoomService
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ConcurrentDictionary<string, List<Func<Task>>> _subscriptions = new();

    public Room? GetRoom(string code) =>
        _rooms.TryGetValue(code.ToUpper(), out var room) ? room : null;

    public (Room Room, Participant Participant) CreateRoom(string hostName, CardDeck deck)
    {
        var code = GenerateCode();
        var host = new Participant { Name = hostName, IsHost = true };
        var room = new Room
        {
            Code = code,
            HostId = host.Id,
            Participants = [host],
            Deck = deck
        };
        _rooms[code] = room;
        return (room, host);
    }

    public (Room Room, Participant Participant)? JoinRoom(string code, string participantName)
    {
        var room = GetRoom(code.ToUpper());
        if (room is null) return null;

        var participant = new Participant { Name = participantName };
        lock (room.Participants)
        {
            room.Participants.Add(participant);
        }
        NotifyRoom(room.Code);
        return (room, participant);
    }

    public void LeaveRoom(string code, string participantId)
    {
        var room = GetRoom(code);
        if (room is null) return;

        lock (room.Participants)
        {
            room.Participants.RemoveAll(p => p.Id == participantId);
        }

        // Transfer host if needed
        if (room.HostId == participantId && room.Participants.Count > 0)
        {
            var newHost = room.Participants[0];
            newHost.IsHost = true;
            room.HostId = newHost.Id;
            room.HostTransferMessage = $"{newHost.Name} is now the Host of this session";
        }

        NotifyRoom(code);
    }

    public void TransferHost(string code, string newHostId)
    {
        var room = GetRoom(code);
        if (room is null) return;

        var currentHost = room.Participants.FirstOrDefault(p => p.IsHost);
        var newHost = room.Participants.FirstOrDefault(p => p.Id == newHostId);
        if (newHost is null) return;

        if (currentHost is not null)
            currentHost.IsHost = false;

        newHost.IsHost = true;
        room.HostId = newHostId;
        room.HostTransferMessage = $"{newHost.Name} is now the Host of this session";

        NotifyRoom(code);
    }

    public void Vote(string code, string participantId, string vote)
    {
        var room = GetRoom(code);
        if (room is null) return;

        var participant = room.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant is null) return;

        participant.CurrentVote = vote;

        if (room.CurrentStory is not null)
            room.CurrentStory.Votes[participantId] = vote;

        NotifyRoom(code);
    }

    public void RevealVotes(string code)
    {
        var room = GetRoom(code);
        if (room is null) return;
        room.VotesRevealed = true;
        NotifyRoom(code);
    }

    public void ResetVotes(string code)
    {
        var room = GetRoom(code);
        if (room is null) return;

        room.VotesRevealed = false;
        foreach (var p in room.Participants)
            p.CurrentVote = null;

        if (room.CurrentStory is not null)
            room.CurrentStory.Votes.Clear();

        NotifyRoom(code);
    }

    public Story AddStory(string code, string title)
    {
        var room = GetRoom(code) ?? throw new InvalidOperationException("Room not found");
        var story = new Story { Title = title };
        lock (room.Stories)
        {
            room.Stories.Add(story);
        }

        // Auto-select if it's the first story
        if (room.CurrentStoryId is null)
        {
            room.CurrentStoryId = story.Id;
        }

        NotifyRoom(code);
        return story;
    }

    public void RemoveStory(string code, string storyId)
    {
        var room = GetRoom(code);
        if (room is null) return;

        lock (room.Stories)
        {
            room.Stories.RemoveAll(s => s.Id == storyId);
        }

        if (room.CurrentStoryId == storyId)
        {
            room.CurrentStoryId = room.Stories.FirstOrDefault()?.Id;
            room.VotesRevealed = false;
            foreach (var p in room.Participants)
                p.CurrentVote = null;
        }

        NotifyRoom(code);
    }

    public void SetCurrentStory(string code, string storyId)
    {
        var room = GetRoom(code);
        if (room is null) return;

        room.CurrentStoryId = storyId;
        room.VotesRevealed = false;

        foreach (var p in room.Participants)
            p.CurrentVote = null;

        // Restore previously cast votes for this story
        var story = room.Stories.FirstOrDefault(s => s.Id == storyId);
        if (story is not null)
        {
            foreach (var p in room.Participants)
            {
                if (story.Votes.TryGetValue(p.Id, out var vote))
                    p.CurrentVote = vote;
            }
        }

        NotifyRoom(code);
    }

    public void SetFinalEstimate(string code, string storyId, string estimate)
    {
        var room = GetRoom(code);
        if (room is null) return;

        var story = room.Stories.FirstOrDefault(s => s.Id == storyId);
        if (story is not null)
        {
            story.FinalEstimate = estimate;
            story.IsComplete = true;
        }

        NotifyRoom(code);
    }

    public void Subscribe(string code, Func<Task> callback)
    {
        _subscriptions.GetOrAdd(code, _ => []).Add(callback);
    }

    public void Unsubscribe(string code, Func<Task> callback)
    {
        if (_subscriptions.TryGetValue(code, out var subs))
            subs.Remove(callback);
    }

    private void NotifyRoom(string code)
    {
        if (_subscriptions.TryGetValue(code, out var subs))
        {
            foreach (var sub in subs.ToList())
                _ = sub();
        }
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
