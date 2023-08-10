namespace Backend.Models.Interfaces;

public interface ILobbyOptions
{
    public uint MaxPlayersLimit { get; }
    public uint MinPlayersLimit { get; }
    public uint MaxPlayers { get; set; }

    public bool IsPublic { get; set; }
}