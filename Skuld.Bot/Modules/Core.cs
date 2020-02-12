using Discord;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Attributes;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, Name("Core")]
    public class CoreModule : ModuleBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => Program.Configuration; }
        private CommandService CommandService { get => MessageHandler.CommandService; }
        public Octokit.GitHubClient GitClient { get; set; }

        [Command("help")]
        [Summary("Gets all commands or a specific command's information")]
        [Usage("help <command>")]
        [Ratelimit(1, 10, Measure.Minutes)]
        public async Task Help([Remainder]string command = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            try
            {
                if (command == null)
                {
                    string title = $"Commands of: {Context.Client.CurrentUser} that can be invoked in: ";

                    if (Context.IsPrivate)
                    {
                        title += $"DMS/{Context.User.Username}#{Context.User.Discriminator}";
                    }
                    else
                    {
                        title += $"{Context.Guild.Name}/{Context.Channel.Name}";
                    }

                    var embed =
                        new EmbedBuilder()
                        .AddAuthor(Context.Client)
                        .WithRandomColor()
                        .WithDescription($"The prefix of **{(Context.Guild == null ? Context.User.FullName() : Context.Guild.Name)}** is: `{(Context.Guild == null ? Configuration.Prefix : (await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix)}`");

                    foreach (var module in CommandService.Modules)
                    {
                        if (Context.IsPrivate) if (module.Name.ToLowerInvariant() == "Admin") continue;

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

                    if(Context.Guild != null)
                    {
                        if (await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false) != null)
                        {
                            var commands = Database.CustomCommands.ToList().Where(x => x.GuildId == Context.Guild.Id);
                            if (commands.Any())
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
                    }

                    var dmchan = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);

                    await embed.QueueMessageAsync(Context, type: Discord.Models.MessageType.DMS).ConfigureAwait(false);
                }
                else
                {
                    var cmd = await CommandService.GetCommandHelpAsync(Context, command).ConfigureAwait(false);
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
                await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                Log.Error("CMD-HELP", ex.Message, ex);
            }
        }

        [Command("issue")]
        [Summary("Create or get an issue on Github")]
        [Usage("issue \"Command x doesn't work\" I tried to run command `x` and it threw the error `error`")]
        [Ratelimit(1, 10, Measure.Minutes)]
        public async Task Issue(string title, [Remainder]string content = null)
        {
            if (!string.IsNullOrEmpty(content))
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var message = new StringBuilder();
                message.AppendLine(content);
                message.AppendLine();
                message.AppendLine("===METADATA===");
                message.AppendLine($"Submitter: {Context.User.Username}");
                message.AppendLine($"Version: Skuld/{SkuldAppContext.Skuld.Key.Version.ToString()}");

                var newMessage =
                    await
                        Context.Client.SendChannelAsync(
                            Context.Client.GetChannel(Configuration.IssueChannel),
                            "",
                            EmbedExtensions.FromMessage("New Issue", $"Issue requires approval, react to approve or deny before posting to github", Context)
                                        .AddField("Content", message.ToString())
                                        .AddField("Submitter", $"{Context.User.FullName()} ({Context.User.Id})")
                                    .Build()
                        ).ConfigureAwait(false);

                await newMessage.AddReactionAsync(DiscordUtilities.Tick_Emote).ConfigureAwait(false);

                await newMessage.AddReactionAsync(DiscordUtilities.Cross_Emote).ConfigureAwait(false);

                Database.Issues.Add(new Issue
                {
                    IssueChannelMessageId = newMessage.Id,
                    Body = message.ToString(),
                    Title = title,
                    SubmitterId = Context.User.Id
                });

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await
                    EmbedExtensions.FromMessage(
                        "Issue Tracker",
                        $"Your issue has been submitted for approval, if it ends up on the [issue tracker]({SkuldAppContext.Skuld.Value.ToString()}/issues) it has been approved",
                        Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
            else
            {
                if (int.TryParse(title, System.Globalization.NumberStyles.Integer, null, out int number))
                {
                    var issue = await GitClient.Issue.Get(Configuration.GithubRepository, number).ConfigureAwait(false);

                    if (issue != null)
                    {
                        var Labels = new StringBuilder();

                        foreach (var label in issue.Labels)
                        {
                            Labels.Append(label.Name);

                            if (label != issue.Labels.LastOrDefault())
                            {
                                Labels.Append(", ");
                            }
                        }

                        await
                            EmbedExtensions.FromMessage(Context)
                            .WithUrl(issue.HtmlUrl)
                            .WithTitle(issue.Title)
                            .WithDescription(issue.Body)
                            .AddInlineField("Labels", Labels.ToString())
                            .AddInlineField("Status", issue.State.StringValue.CapitaliseFirstLetter())
                            .WithColor(issue.State.StringValue[0] == 'o' ? Color.Green : Color.Red)
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await
                            EmbedExtensions.FromError($"Couldn't find an issue with the number: `{title}`", Context)
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    var issues = await GitClient.Issue.GetAllForRepository(Configuration.GithubRepository).ConfigureAwait(false);

                    var issue = issues.FirstOrDefault(x => x.Title.ToUpperInvariant() == title.ToUpperInvariant());

                    if (issue != null)
                    {
                        var Labels = new StringBuilder();

                        foreach (var label in issue.Labels)
                        {
                            Labels.Append(label.Name);

                            if (label != issue.Labels.LastOrDefault())
                            {
                                Labels.Append(", ");
                            }
                        }

                        await
                            EmbedExtensions.FromMessage(Context)
                            .WithUrl(issue.HtmlUrl)
                            .WithTitle(issue.Title)
                            .WithDescription(issue.Body)
                            .AddInlineField("Labels", Labels.ToString())
                            .AddInlineField("Status", issue.State.StringValue.CapitaliseFirstLetter())
                            .WithColor(issue.State.StringValue[0] == 'o' ? Color.Green : Color.Red)
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await
                            EmbedExtensions.FromError($"Couldn't find an issue with the name: `{title}`", Context)
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}