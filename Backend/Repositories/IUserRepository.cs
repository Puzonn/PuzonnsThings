using PuzonnsThings.Models;

namespace TodoApp.Repositories;

public interface IUserRepository
{
    public Task<User?> GetByIdAsync(int id);
    public Task CreateUserAsync(User user);
    public Task UpdateUserAsync(User user);
    public Task DeleteUserAsync(int id);
}
