namespace Backend.Models;

public interface ILobbyCollectable
{
    public int ActivePlayers { get; }
    public uint LobbyId { get; }    
    public DateTime LastLobbySnapshot { get; }
}