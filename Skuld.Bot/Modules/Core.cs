using Discord;
using Discord.Commands;
using Skuld.Bot.Services;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
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
    public class Core : ModuleBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        private CommandService CommandService { get => MessageHandler.CommandService; }

        [Command("help"), Summary("Gets all commands or a specific command's information")]
        public async Task Help([Remainder]string command = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            try
            {
                if (command == null)
                {
                    string prefix = (await Database.GetGuildAsync(Context.Guild) != null ? (await Database.GetGuildAsync(Context.Guild)).Prefix : Configuration.Prefix);

                    string title = $"Commands of: {Context.Client.CurrentUser} that can be invoked in: ";

                    if (Context.IsPrivate)
                    {
                        title += $"DMS/{Context.User.Username}#{Context.User.Discriminator}";
                    }
                    else
                    {
                        title += $"{Context.Guild.Name}/{Context.Channel.Name}";
                    }

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name = title,
                            IconUrl = Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl()
                        },
                        Color = EmbedUtils.RandomColor(),
                        Description = $"The prefix of **{(Context.Guild == null ? Context.User.FullName() : Context.Guild.Name)}** is: `{prefix}`"
                    };
                    foreach (var module in CommandService.Modules)
                    {
                        if (Context.IsPrivate) if (module.Name.ToLowerInvariant() == nameof(Admin).ToLowerInvariant()) continue;

                        string desc = "";
                        foreach (var cmd in module.Commands)
                        {
                            var result = await cmd.CheckPreconditionsAsync(Context).ConfigureAwait(false);
                            if (result.IsSuccess)
                            {
                                desc += $"{cmd.Aliases[0]}, ";
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

                    if (await Database.GetGuildAsync(Context.Guild) != null)
                    {
                        var commands = Database.CustomCommands.Where(x => x.GuildId == Context.Guild.Id);
                        if (commands.Count() > 0)
                        {
                            string commandsText = "";
                            foreach (var cmd in commands)
                            {
                                commandsText += $"{cmd.Name}, ";
                            }
                            commandsText = commandsText[0..^2];
                            embed.AddField("Custom Commands", $"`{commandsText}`");
                        }
                    }

                    var dmchan = await Context.User.GetOrCreateDMChannelAsync();

                    await embed.Build().QueueMessageAsync(Context, Discord.Models.MessageType.DMS).ConfigureAwait(false);
                }
                else
                {
                    var cmd = DiscordUtilities.GetCommandHelp(CommandService, Context, command);
                    if (cmd == null)
                    {
                        await $"Sorry, I couldn't find a command like **{command}**.".QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await cmd.QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await Messages.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                Log.Error("CMD-HELP", ex.Message, ex);
            }
        }
    }
}