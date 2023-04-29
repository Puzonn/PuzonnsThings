namespace PuzonnsThings.Models.WatchTogether;

public class WatchTogetherConnectionRoomCache
{
    private static readonly List<string> _cachedRooms = new List<string>();

    public List<int> ConnectionMembers { get; } = new List<int>();

    public string GroupName { get; }
    public int CreatorId { get; }
    public string CreatorConnectionId { get; set; }

    public bool HasMember(int id) => ConnectionMembers.Any(x => x == id);

    public WatchTogetherConnectionRoomCache(int groupName, int creatorId, string creatorConnectionId)
    {
        GroupName = groupName.ToString();
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
