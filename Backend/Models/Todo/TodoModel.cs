using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Todo;

[Serializable]
public class TodoModel
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("ProgressId")]
    public int ProgressId { get; set; }

    [JsonPropertyName("Date")]
    public string Date { get; set; }

    [JsonPropertyName("UserId")]
    public int UserId { get; set; }

    [JsonPropertyName("Id")]
    public int Id { get; set; }
}