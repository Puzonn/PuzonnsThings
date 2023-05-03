using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;
using PuzonnsThings.Models.WatchTogether;
using PuzonnsThings.Models;

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

        await _context.WatchTogetherRooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }

    public async Task<WatchTogetherRoomApiModel?> GetRoom(int id)
    {
        WatchTogetherRoomModel? room = await _context.WatchTogetherRooms.Where(x => x.Id == id).FirstOrDefaultAsync();

        if (room is null)
        {
            return null;
        }

        WatchTogetherRoomApiModel apiRoom = WatchTogetherRoomApiModel.FromRoomModel(room);

        apiRoom.RoomCreator = _context.Users.Where(x => x.Id == room.CreatorId).FirstOrDefaultAsync().Result.Username;

        return apiRoom;
    }

    public async Task<WatchTogetherRoomApiModel[]> FetchRooms(int count)
    {
        List<WatchTogetherRoomModel> rooms = await _context.WatchTogetherRooms.Take(count).ToListAsync();
        WatchTogetherRoomApiModel[] apiRooms = new WatchTogetherRoomApiModel[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {
            WatchTogetherRoomApiModel model = WatchTogetherRoomApiModel.FromRoomModel(rooms[i]);
            model.RoomCreator = rooms[i].CreatorName;

            apiRooms[i] = model;
        }

        return apiRooms;
    }
}
