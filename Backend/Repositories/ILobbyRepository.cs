using Backend.Models.Lobbies;

namespace Backend.Repositories;

public interface ILobbyRepository
{
    public Task<LobbyModel> AddLobby(LobbyModel lobby);
    public Task RemoveLobby(LobbyModel lobby);
    public Task RemoveLobby(uint lobbyId);
    public Task UpdateLobby(LobbyModel lobby);
    public Task SaveChangesAsync();
    public Task<LobbyModel[]> FetchAllLobbies();
}