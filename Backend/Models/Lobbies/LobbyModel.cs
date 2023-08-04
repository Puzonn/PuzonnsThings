namespace PuzonnsThings.Models.Lobbies;

[Serializable]
public class LobbyModel
{
    public required string LobbyCreator { get; set; }
    public required string LobbyType { get; set; }
    public required int PlayersCount { get; set; }
    public required int MaxPlayersCount { get; set; }
    public required LobbyStatus Status { get; set; }
    public required int CreatorUserId { get; set; }
    public bool LobbyEnded { get; set; } = false;
    public uint Id { get; set; }
}