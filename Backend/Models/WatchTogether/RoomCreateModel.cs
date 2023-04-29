using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.WatchTogether;

public class RoomCreateModel
{
    [Required]
    [JsonPropertyName("VideoTitle")]
    public string VideoTitle { get; set; }

    [JsonPropertyName("VideoAuthor")]
    public string? VideoAuthor { get; set; }

    [Required]
    [JsonPropertyName("VideoId")]
    public string VideoId { get; set; }
}