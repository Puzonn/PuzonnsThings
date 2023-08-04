using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuzonnsThings.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LobbyCreator = table.Column<string>(type: "TEXT", nullable: false),
                    LobbyType = table.Column<string>(type: "TEXT", nullable: false),
                    PlayersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPlayersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatorUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LobbyEnded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskName = table.Column<string>(type: "TEXT", nullable: false),
                    TaskProgressId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskEndDateTime = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Coins = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lobbies");

            migrationBuilder.DropTable(
                name: "TodoList");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
