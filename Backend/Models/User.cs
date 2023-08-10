using System.Diagnostics;
using System.Text.Json.Serialization;

namespace PuzonnsThings.Models;

public class User
{
    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [JsonPropertyName("Password")]
    public string Password { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Balance")]
    public float Balance { get; set; }

    [JsonPropertyName("Avatar")]
    public string Avatar { get; set; }  

    public User(string username, string password, string? email = null)
    {
        Username = username;
        Password = password;
        Email = email;

        Avatar = GenerateRandomAvatar();
        Balance = 0;
    }

    /// <summary>
    /// Generates a random avatar in the form of a hexadecimal color string.
    /// </summary>
    /// <returns>
    /// A hexadecimal color string
    /// </returns>
    public string GenerateRandomAvatar()
    {
        return string.Format("#{0:X6}", Random.Shared.Next(0x1000000));
    }
}
