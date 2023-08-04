using PuzonnsThings.Hubs.Yahtzee;

public interface ILobbyPlace
{
    public Dictionary<int, int> LobbyPlaces { get; }
    public int GetPlayerLobbyPlace(int userId);
    public void RemovePlayerFromLobbyPlace(int userId);
    public bool IsLobbyPlaceOccupied(int placeId);
    public bool IsLobbyPlaceOccupiedByOtherPlayer(int userId, int placeId);
}