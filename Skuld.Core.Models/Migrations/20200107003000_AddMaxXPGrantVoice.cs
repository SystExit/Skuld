using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class AddMaxXPGrantVoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "VoiceExpMinMinutes",
                table: "Configurations",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<ulong>(
                name: "VoiceExpMaxGrant",
                table: "Configurations",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoiceExpMaxGrant",
                table: "Configurations");

            migrationBuilder.AlterColumn<int>(
                name: "VoiceExpMinMinutes",
                table: "Configurations",
                type: "int",
                nullable: false,
                oldClrType: typeof(ulong));
        }
    }
}