using PuzonnsThings.Models.Lobbies;

namespace PuzonnsThings.Repositories;

public interface ILobbyRepository
{
    public Task<LobbyModel> AddLobby(LobbyModel lobby);
    public Task RemoveLobby(uint lobbyId);
    public Task UpdateLobby(LobbyModel lobby);
    public Task SaveChangesAsync();
    public Task<LobbyModel[]> FetchAllLobbies();
}