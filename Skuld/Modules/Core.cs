using Discord;
using Discord.Commands;
using Skuld.Commands;
using Skuld.Services;
using Skuld.Utilities.Discord;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Modules
{
    [Group]
    public class Core : SkuldBase<SkuldCommandContext>
    {
        public DatabaseService Database { get; set; }
        private CommandService CommandService { get => HostService.BotService.messageService.commandService; }

        [Command("help"), Summary("Gets all commands or a specific command's information")]
        public async Task Help([Remainder]string command = null)
        {
            if (command == null)
            {
                string prefix = HostService.Configuration.Discord.Prefix;

                if (await Database.CheckConnectionAsync())
                {
                    var guild = await Database.GetGuildAsync(Context.Guild.Id);
                    if (guild != null)
                        prefix = guild.Prefix;
                }

                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"Commands of: {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.DiscriminatorValue}",
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
                    },
                    Color = EmbedUtils.RandomColor(),
                    Description = $"The prefix of **{Context.Guild.Name}** is: `{prefix}`"
                };
                foreach (var module in CommandService.Modules)
                {
                    string desc = "";
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            desc += $"{cmd.Aliases.First()}, ";
                        else continue;
                    }
                    string description = "";
                    foreach (var str in desc.Split(' ').Distinct())
                        description += str + " ";
                    if (!string.IsNullOrWhiteSpace(description))
                        embed.AddField(module.Name, $"`{description.Remove(description.Length - 3)}`");
                }

                var dmchan = await Context.User.GetOrCreateDMChannelAsync();

                await ReplyAsync(dmchan, Context.Channel, "", embed.Build());
            }
            else
            {
                var cmd = DiscordUtilities.GetCommandHelp(CommandService, Context, command);
                if (cmd == null)
                {
                    await ReplyAsync(Context.Channel, $"Sorry, I couldn't find a command like **{command}**.");
                }
                else
                {
                    await ReplyAsync(Context.Channel, cmd);
                }
            }
        }
    }
}