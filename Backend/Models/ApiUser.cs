namespace Backend.Models;

public class ApiUser
{
    public required string Username { get; set; }
    public required float Coins { get; set; }
    public required int Id { get; set; }
}