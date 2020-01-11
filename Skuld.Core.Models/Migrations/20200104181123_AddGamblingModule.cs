using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class AddGamblingModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogLevel",
                table: "Configurations");

            migrationBuilder.AddColumn<bool>(
                name: "Gambling",
                table: "Modules",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoAssign",
                table: "LevelRewards",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gambling",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "AutoAssign",
                table: "LevelRewards");

            migrationBuilder.AddColumn<int>(
                name: "LogLevel",
                table: "Configurations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}