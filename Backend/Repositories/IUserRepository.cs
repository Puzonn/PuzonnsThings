using PuzonnsThings.Models;

namespace PuzonnsThings.Repositories;

public interface IUserRepository
{
    public Task<User?> GetByIdAsync(int id);
    public Task CreateUserAsync(User user);
    public void UpdateUserAsync(User user);
    public Task DeleteUserAsync(int id);
    public Task SaveChangesAsync();
}
