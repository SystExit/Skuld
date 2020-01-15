using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class GithubIssueChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "IssueChannel",
                table: "Configurations",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IssueChannelMessageId = table.Column<ulong>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    HasSent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropColumn(
                name: "IssueChannel",
                table: "Configurations");
        }
    }
}
