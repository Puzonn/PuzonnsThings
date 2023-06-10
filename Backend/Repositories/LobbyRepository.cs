using Backend.Models.Lobbies;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;

namespace Backend.Repositories;

public class LobbyRepository : ILobbyRepository
{
    private readonly DatabaseContext dbContext;

    public LobbyRepository(DatabaseContext _context)
    {
        dbContext = _context;
    }

    public async Task<LobbyModel> AddLobby(LobbyModel lobby)
    {
        await dbContext.Lobbies.AddAsync(lobby);   
        await dbContext.SaveChangesAsync();

        return lobby;
    }

    public Task<LobbyModel[]> FetchAllLobbies()
    {
        return dbContext.Lobbies.ToArrayAsync();
    }

    public Task<LobbyModel[]> FetchLobbies(LobbyType type)
    {
        return dbContext.Lobbies.Where(x => x.LobbyType == type.Name).ToArrayAsync();
    }

    public Task RemoveLobby(LobbyModel lobby)
    {
        throw new NotImplementedException();
    }

    public Task UpdateLobby(LobbyModel lobby)
    {
        throw new NotImplementedException();
    }
}