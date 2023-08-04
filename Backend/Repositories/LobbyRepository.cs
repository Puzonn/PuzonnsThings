using PuzonnsThings.Models.Lobbies;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;

namespace PuzonnsThings.Repositories;

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

    public async Task<LobbyModel[]> FetchAllLobbies()
    {
        return await dbContext.Lobbies.Where(x => !x.LobbyEnded).ToArrayAsync();
    }

    public Task<LobbyModel[]> FetchLobbies(LobbyType type)
    {
        return dbContext.Lobbies.Where(x => x.LobbyType == type.Name && !x.LobbyEnded).ToArrayAsync();
    }

    public Task<LobbyModel?> GetLobby(uint lobbyId)
    {
        return dbContext.Lobbies.Where(x => x.Id == lobbyId).FirstOrDefaultAsync();
    }

    public async Task RemoveLobby(uint lobbyId)
    {
        LobbyModel? lobby;

        if ((lobby = await dbContext.Lobbies.Where(x => x.Id == lobbyId).FirstOrDefaultAsync()) is not null)
        {
            dbContext.Lobbies.Remove(lobby);
        }
    }

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();

    public async Task UpdateLobby(LobbyModel lobby)
    {
        dbContext.Lobbies.Update(lobby);
        await SaveChangesAsync();
    }
}