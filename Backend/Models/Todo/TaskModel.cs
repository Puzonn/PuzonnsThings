using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Todo;

[Serializable]
public class TaskModel
{
    [JsonPropertyName("TaskName")]
    public string TaskName { get; set; }

    [JsonPropertyName("TaskProgressId")]
    public int TaskProgressId { get; set; }

    [JsonPropertyName("TaskPriority")]
    public int TaskPriority { get; set; }   

    [JsonPropertyName("TaskEndDateTime")]
    public string TaskEndDateTime { get; set; }

    [JsonPropertyName("UserId")]
    public int UserId { get; set; }

    [JsonPropertyName("Id")]
    public int Id { get; set; }
}