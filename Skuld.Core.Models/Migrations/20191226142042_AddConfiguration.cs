using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Skuld.Core.Models.Migrations
{
    public partial class AddConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IsDevelopmentBuild = table.Column<bool>(nullable: false),
                    DiscordToken = table.Column<string>(nullable: true),
                    Prefix = table.Column<string>(nullable: true),
                    AltPrefix = table.Column<string>(nullable: true),
                    Shards = table.Column<ushort>(nullable: false),
                    WebsocketHost = table.Column<string>(nullable: true),
                    WebsocketPort = table.Column<ushort>(nullable: false),
                    WebsocketSecure = table.Column<bool>(nullable: false),
                    PinboardThreshold = table.Column<int>(nullable: false),
                    PinboardDateLimit = table.Column<int>(nullable: false),
                    DailyAmount = table.Column<ulong>(nullable: false),
                    VoiceExpIndeterminate = table.Column<float>(nullable: false),
                    VoiceExpMinMinutes = table.Column<int>(nullable: false),
                    GoogleAPI = table.Column<string>(nullable: true),
                    GoogleCx = table.Column<string>(nullable: true),
                    STANDSUid = table.Column<int>(nullable: false),
                    STANDSToken = table.Column<string>(nullable: true),
                    TwitchToken = table.Column<string>(nullable: true),
                    TwitchClientID = table.Column<string>(nullable: true),
                    ImgurClientID = table.Column<string>(nullable: true),
                    ImgurClientSecret = table.Column<string>(nullable: true),
                    NASAApiKey = table.Column<string>(nullable: true),
                    DataDogHost = table.Column<string>(nullable: true),
                    DataDogPort = table.Column<ushort>(nullable: true),
                    DBotsOrgKey = table.Column<string>(nullable: true),
                    DiscordGGKey = table.Column<string>(nullable: true),
                    B4DToken = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastaVotes",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PastaId = table.Column<ulong>(nullable: false),
                    VoterId = table.Column<ulong>(nullable: false),
                    Upvote = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastaVotes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "PastaVotes");
        }
    }
}