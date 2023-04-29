using PuzonnsThings.Models;

namespace TodoApp.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}
