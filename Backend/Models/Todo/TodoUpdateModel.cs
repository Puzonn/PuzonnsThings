using System.ComponentModel.DataAnnotations;

namespace PuzonnsThings.Models.Todo;

[Serializable]
public class TodoUpdateModel
{
    [Required]
    public int TodoId { get; set; }

    [Required]
    public int ProgressId { get; set; }
}