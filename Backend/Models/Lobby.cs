using Backend.Models.Interfaces;

namespace Backend.Models;

public abstract class Lobby<TLobbyPlayerType, TLobbyOptionsType> : ILobbyCollectable, ILobbyPlace
    where TLobbyPlayerType : IPlayer where TLobbyOptionsType : ILobbyOptions
{
    /// <summary>
    ///     Id of lobby creator
    /// </summary>
    public readonly uint LobbyCreatorId;

    public readonly TLobbyOptionsType LobbyOptions;

    /// <summary>
    ///     Connected players
    /// </summary>
    public readonly List<TLobbyPlayerType> Players = new();

    protected Lobby(uint lobbyCreatorId, uint lobbyId, ILobbyOptions options)
    {
        LobbyId = lobbyId;
        LobbyCreatorId = lobbyCreatorId;
        LobbyOptions = (TLobbyOptionsType)options;
        MaxPlayers = options.MinPlayersLimit;
    }

    public uint MaxPlayers
    {
        get => LobbyOptions.MaxPlayers;
        set => LobbyOptions.MaxPlayers = value;
    }

    /// <summary>
    ///     Count of connected players
    /// </summary>
    public int ActivePlayers => Players.Count;

    /// <summary>
    ///     Id of lobby
    /// </summary>
    public uint LobbyId { get; }

    /// <summary>
    ///     Datetime of last snapshot
    /// </summary>
    public DateTime LastLobbySnapshot { get; }

    /// <summary>
    ///     Dictionary of places where key is userId and value is placeId
    /// </summary>
    public Dictionary<int, int> LobbyPlaces { get; } = new();

    /// <summary>
    ///     Retrieves the lobby place of a player.
    /// </summary>
    /// <param name="userId">The ID of the player.</param>
    /// <returns>
    ///     The lobby place occupied by the player, or -1 if the player is not in any lobby place.
    /// </returns>
    public int GetPlayerLobbyPlace(int userId)
    {
        return LobbyPlaces.ContainsKey(userId) ? LobbyPlaces[userId] : -1;
    }

    /// <summary>
    ///     Chooses a lobby place for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="placeId">The ID of the lobby place to choose.</param>
    /// <returns>
    ///     True if the lobby place was successfully chosen, false if the user already has a lobby place.
    /// </returns
    public bool ChoosePlace(int userId, int placeId)
    {
        if (LobbyPlaces.ContainsKey(userId) || IsLobbyPlaceOccupied(placeId)) return false;

        if (placeId >= LobbyOptions.MaxPlayersLimit) return false;

        LobbyPlaces.Add(userId, placeId);

        return true;
    }

    /// <summary>
    ///     Checks if a lobby place is currently occupied.
    /// </summary>
    /// <param name="placeId">The ID of the lobby place to check.</param>
    /// <returns>
    ///     True if the lobby place is occupied, false otherwise.
    /// </returns
    public bool IsLobbyPlaceOccupied(int placeId)
    {
        return LobbyPlaces.ContainsValue(placeId);
    }

    /// <summary>
    ///     Removes a player from the lobby place.
    /// </summary>
    /// <param name="userId">The ID of the user to be removed from the lobby place.</param>
    public void RemovePlayerFromLobbyPlace(int userId)
    {
        if (GetPlayerLobbyPlace(userId) != -1) LobbyPlaces.Remove(userId);
    }

    /// <summary>
    ///     Changes count of maximum players in game. Dose not check if user is creator.
    /// </summary>
    /// <param name="count">Max players count</param>
    /// <returns>Returns true when count pass validating</returns>
    public bool ChangeMaxPlayers(int state)
    {
        if (state <= 1) return false;

        var validate = state <= LobbyOptions.MaxPlayersLimit && state >= LobbyOptions.MinPlayersLimit;

        if (validate)
        {
            LobbyPlaces.Clear();
            MaxPlayers = (uint)state;
        }

        return validate;
    }

    public void ChangePrivacy(bool state)
    {
        LobbyOptions.IsPublic = state;
    }
}