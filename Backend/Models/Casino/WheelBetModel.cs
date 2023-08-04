using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Casino;

public class WheelBetModel
{
    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    [JsonPropertyName("pointType")]
    public WheelPointType WheelPoint { get; set; }
}