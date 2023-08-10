namespace Backend.Models.Interfaces;

public interface IPlayer
{
    public int UserId { get; }

    public string ConnectionId { get; set; }
    public string Username { get; }
    public string Avatar { get; }

    public bool IsReady { get; set; }
}
