namespace Skuld.Bot.Models
{
    public struct CommandSkuld
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Usage { get => Discord.Handlers.MessageHandler.cmdConfig.Prefix + Name; }
        public string[] Aliases { get; set; }
    }
}
