namespace Skuld.Discord.Models
{
    public enum AccessLevel
    {
        Blocked = -1,
        User = 0,
        ServerMod = 1,
        ServerAdmin = 2,
        ServerOwner = 3
    }

    public enum BotAccessLevel
    {
        Normal = 0,
        BotTester = 1,
        BotDonator = 2,
        BotAdmin = 3,
        BotOwner = 4
    }
}