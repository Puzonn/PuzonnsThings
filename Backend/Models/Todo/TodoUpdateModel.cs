using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Todo;

[Serializable]
public class TodoUpdateModel
{
    [Required]
    [JsonPropertyName("TaskId")]
    public int TaskId { get; set; }

    [Required]
    [JsonPropertyName("TaskName")]
    public string TaskName { get; set; }

    [Required]
    [JsonPropertyName("TaskPriority")]
    public int TaskPriority { get; set; }

    [Required]
    [JsonPropertyName("TaskEndDateTime")]
    public string TaskEndDateTime { get; set; }

    [Required]
    [JsonPropertyName("TaskProgressId")]
    public int TaskProgressId { get; set; }
}