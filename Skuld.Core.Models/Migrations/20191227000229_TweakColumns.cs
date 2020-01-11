using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class TweakColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banned",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GlaredAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Glares",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HP",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Banned",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "GlaredAt",
                table: "Users",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "Glares",
                table: "Users",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "HP",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}