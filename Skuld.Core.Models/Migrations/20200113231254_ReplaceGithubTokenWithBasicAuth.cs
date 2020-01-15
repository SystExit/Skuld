using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class ReplaceGithubTokenWithBasicAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubClientToken",
                table: "Configurations");

            migrationBuilder.AddColumn<string>(
                name: "GithubClientPassword",
                table: "Configurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GithubClientUsername",
                table: "Configurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubClientPassword",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "GithubClientUsername",
                table: "Configurations");

            migrationBuilder.AddColumn<string>(
                name: "GithubClientToken",
                table: "Configurations",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
