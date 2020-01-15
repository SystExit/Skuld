using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class AddGithubClientToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "UnlockedCustBG",
                table: "Users",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RecurringBlock",
                table: "Users",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BanReason",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Background",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "UserCommandUsage",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Upvote",
                table: "PastaVotes",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pastas",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Pastas",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Weeb",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Stats",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Space",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Search",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Lewd",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Information",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Gambling",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Fun",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Custom",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Admin",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Actions",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Accounts",
                table: "Modules",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoneyName",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoneyIcon",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LevelUpMessage",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeaveMessage",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JoinMessage",
                table: "Guilds",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Pinning",
                table: "Features",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Experience",
                table: "Features",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomCommands",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CustomCommands",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "WebsocketSecure",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "WebsocketHost",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TwitchToken",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TwitchClientID",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STANDSToken",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NASAApiKey",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDevelopmentBuild",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImgurClientSecret",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImgurClientID",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleCx",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleAPI",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiscordToken",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiscordGGKey",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DataDogHost",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DBotsOrgKey",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B4DToken",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltPrefix",
                table: "Configurations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<string>(
                name: "GithubClientToken",
                table: "Configurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubClientToken",
                table: "Configurations");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UnlockedCustBG",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "RecurringBlock",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BanReason",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Background",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "UserCommandUsage",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Upvote",
                table: "PastaVotes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pastas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Pastas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Weeb",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Stats",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Space",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Search",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Lewd",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Information",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Gambling",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Fun",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Custom",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Admin",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Actions",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Accounts",
                table: "Modules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoneyName",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoneyIcon",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LevelUpMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeaveMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JoinMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "Pinning",
                table: "Features",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<ulong>(
                name: "Experience",
                table: "Features",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomCommands",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CustomCommands",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "WebsocketSecure",
                table: "Configurations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "WebsocketHost",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TwitchToken",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TwitchClientID",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STANDSToken",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NASAApiKey",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "IsDevelopmentBuild",
                table: "Configurations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "ImgurClientSecret",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImgurClientID",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleCx",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleAPI",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiscordToken",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiscordGGKey",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DataDogHost",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DBotsOrgKey",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B4DToken",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltPrefix",
                table: "Configurations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Configurations",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
