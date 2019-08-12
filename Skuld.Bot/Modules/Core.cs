using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Core.Models.Skuld;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Core : InteractiveBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        private CommandService CommandService { get => MessageHandler.CommandService; }

        [Command("help"), Summary("Gets all commands or a specific command's information")]
        public async Task Help([Remainder]string command = null)
        {
            try
            {
                if (command == null)
                {
                    string prefix = (Context.DBGuild != null ? Context.DBGuild.Prefix : Configuration.Discord.Prefix);

                    string title = $"Commands of: {Context.Client.CurrentUser.Username}#{Context.Client.CurrentUser.DiscriminatorValue} that can be invoked in: ";

                    if (Context.IsPrivate)
                    {
                        title += $"DMS/{Context.User.Username}#{Context.User.Discriminator}";
                    }
                    else
                    {
                        title += $"**{Context.Guild.Name}/{Context.Channel.Name}**";
                    }

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name = title,
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
                            {
                                desc += $"{cmd.Aliases.First()}, ";
                            }
                            else continue;
                        }
                        string description = "";
                        foreach (var str in desc.Split(' ').Distinct())
                        {
                            description += str + " ";
                        }
                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            embed.AddField(module.Name, $"`{description.Remove(description.Length - 3)}`");
                        }
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

                    await embed.Build().QueueMessage(Discord.Models.MessageType.DMS, Context.User, Context.Channel);
                }
                else
                {
                    var cmd = DiscordUtilities.GetCommandHelp(CommandService, Context, command);
                    if (cmd == null)
                    {
                        await $"Sorry, I couldn't find a command like **{command}**.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await cmd.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-HELP", ex.Message, LogSeverity.Error, ex));
            }
        }
    }
}