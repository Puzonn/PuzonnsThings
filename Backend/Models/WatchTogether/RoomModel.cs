using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.WatchTogether;

[Serializable]
public class WatchTogetherRoomModel
{
    public int Id { get; set; }
    public int RoomWatchers { get; set; }
    public string VideoTitle { get; set; }
    public string VideoId { get; set; }
    public int CreatorId { get; set; }
    public string CreatorName { get; set; }
    public DateTime CreationTime { get; set; }
}

[Serializable]
public class WatchTogetherRoomApiModel
{
    [JsonPropertyName("RoomId")]
    public int RoomId { get; set; }

    [JsonPropertyName("RoomWatchers")]
    public int RoomWatchers { get; set; }

    [JsonPropertyName("RoomCreator")]
    public string RoomCreator { get; set; }

    [JsonPropertyName("VideoTitle")]
    public string VideoTitle { get; set; }

    [JsonPropertyName("VideoId")]
    public string VideoId { get; set; }

    public static WatchTogetherRoomApiModel FromRoomModel(WatchTogetherRoomModel model) => new WatchTogetherRoomApiModel()
    {
        RoomId = model.Id,
        VideoId = model.VideoId,
        VideoTitle = model.VideoTitle,
        RoomWatchers = model.RoomWatchers,
    };
}