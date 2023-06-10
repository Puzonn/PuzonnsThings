using System.Text.Json.Serialization;

namespace Backend.Models.Casino;

public class WheelBetModel
{
    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    [JsonPropertyName("pointType")]
    public WheelPointType WheelPoint { get; set; }
}