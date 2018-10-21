using Discord;
using Discord.Commands;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord;
using Skuld.Discord.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Core : SkuldBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        private CommandService CommandService { get => BotService.CommandService; }

        [Command("help"), Summary("Gets all commands or a specific command's information")]
        public async Task Help([Remainder]string command = null)
        {
            if (command == null)
            {
                string prefix = Context.DBGuild.Prefix ?? Configuration.Discord.Prefix;

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

                var allCommandsResp = await DatabaseClient.GetAllCustomCommandsAsync(Context.Guild.Id);
                if (allCommandsResp.Successful)
                {
                    var commands = allCommandsResp.Data as IReadOnlyList<CustomCommand>;
                    string commandsText = "";
                    foreach (var cmd in commands)
                    {
                        commandsText += $"{cmd.CommandName}, ";
                    }
                    commandsText = commandsText.Substring(0, commandsText.Length - 2);
                    embed.AddField("Custom Commands", $"`{commandsText}`");
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