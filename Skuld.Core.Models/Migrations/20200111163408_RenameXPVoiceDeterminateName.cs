using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class RenameXPVoiceDeterminateName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoiceExpIndeterminate",
                table: "Configurations");

            migrationBuilder.AddColumn<float>(
                name: "VoiceExpDeterminate",
                table: "Configurations",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoiceExpDeterminate",
                table: "Configurations");

            migrationBuilder.AddColumn<float>(
                name: "VoiceExpIndeterminate",
                table: "Configurations",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
