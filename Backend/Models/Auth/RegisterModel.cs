using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Auth;

[Serializable]
public class RegisterModel
{
    [Required]
    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [Required]
    [JsonPropertyName("Password")]
    public string Password { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Username) || Username.Length < 5) return false;
        if (string.IsNullOrEmpty(Password) || Password.Length < 5) return false;

        return true;
    }
}
