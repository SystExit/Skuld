using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class AddGithubRepository : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GithubRepository",
                table: "Configurations",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GithubRepository",
                table: "Configurations");
        }
    }
}
