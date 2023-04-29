using Newtonsoft.Json;

namespace PuzonnsThings.Models.WatchTogether;

[Serializable]
public class WatchTogetherSyncStateModel
{
    [JsonProperty("State")]
    public string State { get; set; }

    [JsonProperty("Data")]
    public object Data { get; set; }

    [JsonProperty("IgnoreCreator")]
    public bool IgnoreCreator { get; set; } = true;
}
