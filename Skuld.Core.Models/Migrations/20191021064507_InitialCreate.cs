using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            migrationBuilder.CreateTable(
                name: "BlockedActions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Blocker = table.Column<ulong>(nullable: false),
                    Blockee = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomCommands",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true, unicode: true),
                    Content = table.Column<string>(nullable: true, unicode: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Pinning = table.Column<bool>(nullable: false),
                    Experience = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JoinMessage = table.Column<string>(nullable: true, unicode: true),
                    LeaveMessage = table.Column<string>(nullable: true, unicode: true),
                    JoinRole = table.Column<ulong>(nullable: false),
                    Prefix = table.Column<string>(nullable: true, unicode: true),
                    MutedRole = table.Column<ulong>(nullable: false),
                    JoinChannel = table.Column<ulong>(nullable: false),
                    LeaveChannel = table.Column<ulong>(nullable: false),
                    LevelUpChannel = table.Column<ulong>(nullable: false),
                    LevelUpMessage = table.Column<string>(nullable: true),
                    LevelNotification = table.Column<int>(nullable: false),
                    MoneyName = table.Column<string>(nullable: true, unicode: true),
                    MoneyIcon = table.Column<string>(nullable: true, unicode: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IAmRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(nullable: false),
                    RoleId = table.Column<ulong>(nullable: false),
                    Price = table.Column<int>(nullable: false),
                    LevelRequired = table.Column<int>(nullable: false),
                    RequiredRoleId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAmRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LevelRewards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildId = table.Column<ulong>(nullable: false),
                    RoleId = table.Column<ulong>(nullable: false),
                    LevelRequired = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelRewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Accounts = table.Column<bool>(nullable: false),
                    Actions = table.Column<bool>(nullable: false),
                    Admin = table.Column<bool>(nullable: false),
                    Custom = table.Column<bool>(nullable: false),
                    Fun = table.Column<bool>(nullable: false),
                    Information = table.Column<bool>(nullable: false),
                    Lewd = table.Column<bool>(nullable: false),
                    Search = table.Column<bool>(nullable: false),
                    Space = table.Column<bool>(nullable: false),
                    Stats = table.Column<bool>(nullable: false),
                    Weeb = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pastas",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true, unicode: true),
                    Created = table.Column<ulong>(nullable: false),
                    Content = table.Column<string>(nullable: true, unicode: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pastas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reputations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Repee = table.Column<ulong>(nullable: false),
                    Reper = table.Column<ulong>(nullable: false),
                    Timestamp = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reputations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCommandUsage",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(nullable: false),
                    Usage = table.Column<ulong>(nullable: false),
                    Command = table.Column<string>(nullable: true, unicode: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCommandUsage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(nullable: true),
                    Money = table.Column<ulong>(nullable: false),
                    Title = table.Column<string>(nullable: true, unicode: true),
                    Description = table.Column<string>(nullable: true, unicode: true),
                    LastDaily = table.Column<ulong>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Patted = table.Column<ulong>(nullable: false),
                    Pats = table.Column<ulong>(nullable: false),
                    HP = table.Column<int>(nullable: false),
                    GlaredAt = table.Column<ulong>(nullable: false),
                    Glares = table.Column<ulong>(nullable: false),
                    AvatarUrl = table.Column<string>(nullable: true),
                    Banned = table.Column<bool>(nullable: false),
                    BanReason = table.Column<string>(nullable: true, unicode: true),
                    RecurringBlock = table.Column<bool>(nullable: false),
                    UnlockedCustBG = table.Column<bool>(nullable: false),
                    Background = table.Column<string>(nullable: true, defaultValue: "#3F51B5")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserXp",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    Level = table.Column<ulong>(nullable: false),
                    XP = table.Column<ulong>(nullable: false),
                    TotalXP = table.Column<ulong>(nullable: false),
                    LastGranted = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserXp", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedActions");

            migrationBuilder.DropTable(
                name: "CustomCommands");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "IAmRoles");

            migrationBuilder.DropTable(
                name: "LevelRewards");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Pastas");

            migrationBuilder.DropTable(
                name: "Reputations");

            migrationBuilder.DropTable(
                name: "UserCommandUsage");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserXp");
        }
    }
}