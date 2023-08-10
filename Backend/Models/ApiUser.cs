namespace PuzonnsThings.Models;

public class ApiUser
{
    public required string Username { get; set; }
    public required float Coins { get; set; }
    public required string Avatar { get; set; }
    public required int UserId { get; set; }
}