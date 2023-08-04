using PuzonnsThings.Databases;
using PuzonnsThings.Models;

namespace PuzonnsThings.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _dbContext;

    public UserRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task CreateUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public async void UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);

        await SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);

        if (user != null)
        {
            _dbContext.Users.Remove(user);
        }
    }

    public async Task AddCoins(int userId, float coins)
    {
        User? user = await GetByIdAsync(userId);

        if (user is null)
        {
            return;
        }

        user.Coins += coins;
        _dbContext.Update(user);
    }

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
