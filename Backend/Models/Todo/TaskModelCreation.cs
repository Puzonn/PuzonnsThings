using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Todo;

[Serializable]
public class TaskModelCreation
{
    [Required]
    [JsonPropertyName("TaskName")]
    public string TaskName { get; set; }

    [Required]
    [JsonPropertyName("TaskPriority")]
    public int TaskPriority { get; set; }

    [Required]
    [JsonPropertyName("TaskEndDateTime")]
    public string TaskEndDateTime { get; set; }

    public bool Validate(out string error)
    {
        if (string.IsNullOrEmpty(TaskName) || string.IsNullOrEmpty(TaskEndDateTime))
        {
            error = "Some of property was empty";
            return false;
        }

        if (!DateTime.TryParse(TaskEndDateTime, out DateTime startTime))
        {
            error = "Parsing date time problem";
            return false;
        }

        error = "";
        return true;
    }
}