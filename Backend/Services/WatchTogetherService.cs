using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using PuzonnsThings.Models.WatchTogether;

namespace PuzonnsThings.Services;

public class WatchTogetherService
{
    private static readonly Dictionary<int, WatchTogetherConnectionRoomCache> cachedRooms = new Dictionary<int, WatchTogetherConnectionRoomCache>();

    private readonly DatabaseContext _context;

    public WatchTogetherService(DatabaseContext context)
    {
        _context = context;
    }

    public WatchTogetherConnectionRoomCache[] GetCachedRooms() => cachedRooms.Values.ToArray();

    public WatchTogetherConnectionRoomCache? GetByMember(int userId) => cachedRooms.FirstOrDefault(x => x.Value.HasMember(userId)).Value;

    public WatchTogetherConnectionRoomCache? GetByCreator(int userId) => cachedRooms.FirstOrDefault(x => x.Value.CreatorId == userId).Value;

    public WatchTogetherConnectionRoomCache? GetByRoomId(int roomId)
    {
        cachedRooms.TryGetValue(roomId, out WatchTogetherConnectionRoomCache? room);
        return room;
    }

    public void AddCachedRoom(WatchTogetherConnectionRoomCache room)
    {
        cachedRooms.Add(room.GroupId, room);
    }

    public void UpdateConnectionId(int roomId, string connectionId)
    {
        cachedRooms[roomId].CreatorConnectionId = connectionId;
    }

    public async Task CreateRoom(RoomCreateModel createModel, User user)
    {
        WatchTogetherRoomModel room = new WatchTogetherRoomModel()
        {
            VideoId = createModel.VideoId,
            VideoTitle = createModel.VideoTitle,
            CreatorId = user.Id,
            RoomWatchers = 0,
            CreatorName = user.Username,
            CreationTime = DateTime.UtcNow,
        };
    }
}
