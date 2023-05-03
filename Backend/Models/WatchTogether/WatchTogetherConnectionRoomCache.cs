namespace PuzonnsThings.Models.WatchTogether;

public class WatchTogetherConnectionRoomCache
{
    public List<int> ConnectionMembers { get; } = new List<int>();

    public int GroupId { get; }
    public int CreatorId { get; }
    public string CreatorConnectionId { get; set; }

    public bool HasMember(int id) => ConnectionMembers.Any(x => x == id);

    public WatchTogetherConnectionRoomCache(int groupId, int creatorId, string creatorConnectionId)
    {
        GroupId = groupId;
        CreatorId = creatorId;
        CreatorConnectionId = creatorConnectionId;

        AddMember(creatorId);
    }

    public void AddMember(int userId)
    {
        if (!ConnectionMembers.Contains(userId))
        {
            ConnectionMembers.Add(userId);
        }
    }
}
