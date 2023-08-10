public interface ILobbyPlace
{
    public Dictionary<int, int> LobbyPlaces { get; }
    public int GetPlayerLobbyPlace(int userId);
    public void RemovePlayerFromLobbyPlace(int userId);
    public bool IsLobbyPlaceOccupied(int placeId);
    public bool ChoosePlace(int userId, int placeId);
}