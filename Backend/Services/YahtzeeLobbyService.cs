using PuzonnsThings.Hubs.Yahtzee;
using PuzonnsThings.Models.Lobbies;
using PuzonnsThings.Models.Yahtzee;
using PuzonnsThings.Repositories;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;

namespace PuzonnsThings.Services;

public class YahtzeeService
{
    private static readonly Dictionary<int, YahtzeePlayer> Users = new Dictionary<int, YahtzeePlayer>();
    private static readonly Dictionary<uint, YahtzeeLobby> Rooms = new Dictionary<uint, YahtzeeLobby>();

    private readonly DatabaseContext dbContext;
    private readonly LobbyRepository _lobbyRepository;

    public YahtzeeService(DatabaseContext context, LobbyRepository lobbyRepository)
    {
        dbContext = context;
        _lobbyRepository = lobbyRepository;
    }

    public Dictionary<int, YahtzeePlayer> GetPlayers => Users;

    public void RemoveFromMemory(YahtzeeLobby lobby)
    {
        Rooms.Remove(lobby.LobbyId);
    }

    public async Task UpdateLobbyPlayerCount(uint lobbyId)
    {
        LobbyModel? lobby = await _lobbyRepository.GetLobby(lobbyId);

        if (lobby is null)
        {
            throw new InvalidOperationException("UpdateLobbyPlayerCount lobby was null");
        }

        lobby.PlayersCount = Rooms[lobbyId].Players.Count;

        await _lobbyRepository.UpdateLobby(lobby);
    }

    public async void Remove(uint lobbyId)
    {
        Rooms.Remove(lobbyId);

        var lobbyModel = await dbContext.Lobbies.Where(x => x.Id == lobbyId).FirstOrDefaultAsync();

        if (lobbyModel is not null)
        {
            dbContext.Lobbies.Remove(lobbyModel);
        }
    }

    public async Task<LobbyModel?> GetLobbyModel(uint lobbyId)
    {
        return await dbContext.Lobbies.Where(x => x.Id == lobbyId).FirstOrDefaultAsync();
    }

    public void AddRoom(uint lobbyId, YahtzeeLobby lobby)
    {
        Rooms.Add(lobbyId, lobby);
    }

    public void AddPlayer(int userId, YahtzeePlayer player)
    {
        Users.Add(userId, player);
    }

    public bool HasPlayer(int userId)
    {
        return Users.ContainsKey(userId);
    }

    public bool HasLobby(uint lobbyId)
    {
        return Rooms.ContainsKey(lobbyId);
    }

    public bool HasLobby(uint lobbyId, out YahtzeeLobby? lobby)
    {
        return Rooms.TryGetValue(lobbyId, out lobby);
    }

    public async Task EndGame(YahtzeeLobby yahtzeeLobby, YahtzeeEndGameModel endgame)
    {
        yahtzeeLobby.GameEnded = true;
        LobbyModel? lobby = await _lobbyRepository.GetLobby(yahtzeeLobby.LobbyId);

        if (lobby is null)
        {
            throw new InvalidOperationException("EndGame lobby was null");
        }

        lobby.LobbyEnded = true;
        yahtzeeLobby.GameEnded = true;
    }

    public YahtzeePlayer GetPlayer(int userId)
    {
        return Users[userId];
    }

    public YahtzeeLobby? GetLobby(uint lobbyId)
    {
        return Rooms.ContainsKey(lobbyId) ? Rooms[lobbyId] : null;
    }
}