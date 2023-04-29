using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Models;
using PuzonnsThings.Models.Todo;
using PuzonnsThings.Models.WatchTogether;
using PuzonnsThings.Models.Yahtzee;

namespace PuzonnsThings.Databases;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoModel> TodoList => Set<TodoModel>();
    public DbSet<WatchTogetherRoomModel> WatchTogetherRooms => Set<WatchTogetherRoomModel>();
    public DbSet<YahtzeeRoomModel> YahtzeeRooms => Set<YahtzeeRoomModel>();
}
