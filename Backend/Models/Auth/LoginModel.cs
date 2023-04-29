using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models.Auth;

public class LoginModel
{
    [Required]
    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [Required]
    [JsonPropertyName("Password")]
    public string Password { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Username) || Username.Length < 5) return false;
        if (string.IsNullOrEmpty(Password) || Password.Length < 5) return false;

        return true;
    }
}
