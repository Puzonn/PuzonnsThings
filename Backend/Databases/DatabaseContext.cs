using PuzonnsThings.Models.Lobbies;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Models;
using PuzonnsThings.Models.Todo;

namespace PuzonnsThings.Databases;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskModel> TodoList => Set<TaskModel>();
    public DbSet<LobbyModel> Lobbies => Set<LobbyModel>();
}