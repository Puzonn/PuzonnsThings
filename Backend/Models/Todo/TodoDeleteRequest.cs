using System.ComponentModel.DataAnnotations;


namespace PuzonnsThings.Models.Todo;

[Serializable]
public class TodoDeleteRequest
{
    [Required]
    public int Id { get; set; }
}