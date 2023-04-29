using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.WatchTogether;

[Serializable]
public class WatchTogetherSyncModel
{
    [JsonPropertyName("CurrentTime")]
    public float CurrentTime { get; set; }

    [JsonPropertyName("IsPaused")]
    public bool IsPaused { get; set; }
}