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

    [JsonPropertyName("Coins")]
    public float Coins { get; set; } = 0;

    public User(string username, string password, string? email = null)
    {
        Username = username;
        Password = password;
        Email = email;
    }
}
