using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NCalc;
using NodaTime;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Services.Bot;
using Skuld.Services.Discord.Attributes;
using Skuld.Services.Discord.Preconditions;
using Skuld.Services.Extensions;
using Skuld.Services.Globalization;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, Name("Information"), RequireEnabledModule]
    public class InformationModule : ModuleBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        public Locale Locale { get; set; }

        [Command("server"), Summary("Gets information about the server")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetServer()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var dbguild = await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);

            Embed embed = await Context.Guild.GetSummaryAsync(Context.Client, Context, dbguild).ConfigureAwait(false);

            await embed.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("server-emojis"), RequireContext(ContextType.Guild)]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task ServerEmoji()
        {
            var guild = Context.Guild;
            string message = null;
            var num = 0;
            message += $"Emojis of __**{guild.Name}**__ ({guild.Emotes.Count})\n" + Environment.NewLine;
            if (guild.Emotes.Count != 0)
            {
                foreach (var emoji in guild.Emotes)
                {
                    num++;
                    if (num % 5 != 0 || num == 0)
                    { message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}> | "; }
                    else
                    { message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}>\n"; }
                }
                message = message[0..^2];
            }
            await message.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("server-roles"), RequireContext(ContextType.Guild)]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task ServerRoles()
        {
            var guild = Context.Guild;
            var roles = guild.Roles.OrderByDescending(x => x.Position);
            string serverroles = null;
            foreach (var item in roles)
            {
                string thing = item.Name.TrimStart('@');

                serverroles += thing;

                if (item != roles.Last())
                {
                    serverroles += ", ";
                }
            }
            string message = null;
            message += $"Roles of __**{guild.Name}**__ ({roles.Count()})\n" + Environment.NewLine;
            if (roles.Any())
            {
                message += "`" + serverroles + "`";
            }
            await message.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("id-guild"), Summary("Get ID of Guild"), RequireContext(ContextType.Guild)]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GuildID() =>
            await $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`"
                .QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("id"), Summary("Gets a users ID")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetID(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            await $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`"
                .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("id"), Summary("Get id of channel"), RequireContext(ContextType.Guild)]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task ChanID(IChannel channel) =>
            await $"The ID of **{channel?.Name}** is `{channel.Id}`".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("roleinfo")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task RoleInfo([Remainder] IRole role)
        {
            var memberString = new StringBuilder();

            var orderedRoles = Context.Guild.Roles.OrderBy(x => x.Position);

            IRole previousRole = null;
            if (role != orderedRoles.FirstOrDefault())
            {
                previousRole = orderedRoles.ElementAtOrDefault(role.Position - 1);
            }
            IRole nextRole = null;
            if (role != orderedRoles.LastOrDefault())
            {
                nextRole = orderedRoles.ElementAtOrDefault(role.Position + 1);
            }

            var members = await Context.Guild.GetRoleMembersAsync(role).ConfigureAwait(false);

            if (role != Context.Guild.EveryoneRole)
            {
                foreach (var member in members)
                {
                    memberString.Append(member.Mention);

                    if (member != members.LastOrDefault())
                    {
                        memberString.Append(", ");
                    }
                }
            }
            else
            {
                memberString.Append(role.Mention);
                memberString.Append(" ");
                memberString.Append("😝");
            }

            await
                new EmbedBuilder()
                    .AddFooter(Context)
                    .WithTitle(role.Name)
                    .WithColor(role.Color)
                    .AddAuthor(Context.Client)
                    .AddInlineField("Hoisted", role.IsHoisted)
                    .AddInlineField("Managed By Discord", role.IsManaged)
                    .AddInlineField("Mentionable", role.IsMentionable)
                    .AddInlineField("Position", $"{role.Position}{(previousRole != null ? $"\nBelow {previousRole.Mention}({previousRole.Position})" : "")}{(nextRole != null ? $"\nAbove {nextRole.Mention}({nextRole.Position})" : "")}")
                    .AddInlineField("Color", role.Color.ToHex())
                    .AddField("Members", memberString.Length <= 1024 ? memberString.ToString() : members.Count.ToFormattedString())
                    .AddField("Created", role.CreatedAt)
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("screenshare"), Summary("Get's the screenshare channel link"), RequireContext(ContextType.Guild), RequireGuildVoiceChannel]
        [Alias("sc")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Screenshare()
            => await $"<https://discordapp.com/channels/{Context.Guild.Id}/{(Context.User as IGuildUser)?.VoiceChannel.Id}>".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("support"), Summary("Gives discord invite")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task DevDisc()
            => await $"Join the support server at: https://discord.skuldbot.uk/discord?ref=bot"
            .QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.DMS).ConfigureAwait(false);

        [Command("invite"), Summary("OAuth2 Invite")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task BotInvite()
            => await $"Invite me using: https://discord.skuldbot.uk/bot?ref=bot".QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.DMS).ConfigureAwait(false);

        [Command("userratio"), Summary("Gets the ratio of users to bots")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task HumanToBotRatio()
        {
            var guild = Context.Guild as SocketGuild;
            var bots = await guild.RobotMembersAsync().ConfigureAwait(false);
            var users = await guild.HumanMembersAsync().ConfigureAwait(false);
            var ratio = await guild.GetBotUserRatioAsync().ConfigureAwait(false);
            var usercount = guild.Users.Count;
            await $"Current Bots are: {bots}\nCurrent Users are: {users}\nTotal Guild Users: {usercount}\n{ratio}% of the Guild Users are bots"
                .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("avatar"), Summary("Gets your avatar url")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Avatar([Remainder]IGuildUser user = null)
        {
            if (user == null)
                user = Context.Guild.GetUser(Context.User.Id);

            var avatar = user.GetAvatarUrl(ImageFormat.Auto, 512) ?? user.GetDefaultAvatarUrl();

            await
                EmbedExtensions.FromMessage(
                    "",
                    $"Avatar for {user.Mention}",
                    Context
                )
                .WithImageUrl(avatar)
                .WithColor(user.GetHighestRoleColor(Context.Guild))
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("mods"), Summary("Gives online status of Moderators/Admins")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task ModsOnline()
        {
            var guild = Context.Guild as SocketGuild;
            await guild.DownloadUsersAsync().ConfigureAwait(false);
            List<string> admins = new List<string>();
            List<string> mods = new List<string>();
            foreach (var user in guild.Users)
            {
                if (user.IsBot) { }
                else
                {
                    if (user.GuildPermissions.Administrator)
                    {
                        if (user.Activity != null)
                        {
                            if (user.Activity.Type == ActivityType.Streaming)
                                admins.Add(DiscordUtilities.Streaming_Emote + $" {user.FullNameWithNickname()}");
                        }
                        else
                        {
                            admins.Add($"{user.Status.StatusToEmote()} {user.FullNameWithNickname()}");
                        }
                    }
                    else if (user.GuildPermissions.RawValue == DiscordUtilities.ModeratorPermissions.RawValue)
                    {
                        if (user.Activity != null)
                        {
                            if (user.Activity.Type == ActivityType.Streaming)
                                mods.Add(DiscordUtilities.Streaming_Emote + $" {user.FullNameWithNickname()}");
                        }
                        else
                        {
                            mods.Add($"{user.Status.StatusToEmote()} {user.FullNameWithNickname()}");
                        }
                    }
                }
            }

            StringBuilder message = new StringBuilder();

            if (admins.Any())
            {
                message.Append("__Administrators__");
                message.Append(Environment.NewLine);
                message.AppendJoin(Environment.NewLine, admins);
            }
            if (mods.Any())
            {
                if (admins.Any())
                {
                    message.Append(Environment.NewLine);
                }
                message.Append("__Moderators__");
                message.AppendJoin(Environment.NewLine, mods);
            }

            await message.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("createinvite", RunMode = RunMode.Async), Summary("Creates a new invite to the guild")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task NewInvite(ITextChannel channel, int maxAge = 0, int maxUses = 0, bool permanent = true, bool unique = true)
        {
            IInviteMetadata invite;
            if (maxAge > 0 && maxUses < 0)
            {
                invite = await channel.CreateInviteAsync(maxAge, null, permanent, unique).ConfigureAwait(false);
            }
            else if (maxAge < 0 && maxUses > 0)
            {
                invite = await channel.CreateInviteAsync(null, maxUses, permanent, unique).ConfigureAwait(false);
            }
            else if (maxAge < 0 && maxUses < 0)
            {
                invite = await channel.CreateInviteAsync(null, null, permanent, unique).ConfigureAwait(false);
            }
            else
            {
                invite = await channel.CreateInviteAsync(maxAge, maxUses, permanent, unique).ConfigureAwait(false);
            }

            await ("I created the invite with the following settings:\n" +
                "```cs\n" +
                $"       Channel : {channel.Name}\n" +
                $"   Maximum Age : {maxAge}\n" +
                $"  Maximum Uses : {maxUses}\n" +
                $"     Permanent : {permanent}\n" +
                $"        Unique : {unique}" +
                $"```Here's the link: {invite.Url}").QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("me")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Whois()
        {
            if (!Context.IsPrivate)
            {
                await GetProileAsync(Context.User as IGuildUser).ConfigureAwait(false);
                return;
            }
            else
            {
                await (await Context.User.GetWhoisAsync(null, null, Context.Client, Configuration).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
        }

        [Command("whois"), Summary("Get's information about a user"), Alias("user")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetProileAsync([Remainder]IGuildUser whois = null)
        {
            if (!Context.IsPrivate)
            {
                if (whois == null)
                    whois = (IGuildUser)Context.User;

                await (await whois.GetWhoisAsync(whois, whois.RoleIds, Context.Client, Configuration).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await (await Context.User.GetWhoisAsync(null, null, Context.Client, Configuration).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("roles"), Summary("Gets a users current roles")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetRole(IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser)Context.User;

            var guild = Context.Guild;
            var userroles = user.RoleIds;
            var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            await $"Roles of __**{user.Username}#{user.Discriminator} ({user.Nickname})**__ ({userroles.Count})\n\n`{(roles ?? "No roles")}`".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("calc"), Summary("Calculates an expression")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Calculate([Remainder] string expression)
        {
            Expression exp = new Expression(expression);

            exp.Parameters.Add("Pi", Math.PI);
            exp.Parameters.Add("e", Math.E);

            var res = exp.Evaluate();

            var result = res;

            if (result is string)
            {
                result = (result as string).Replace("everyone", "\u00A0everyone");
            }

            await
                EmbedExtensions.FromMessage(
                    "Calculator",
                    $"Expression: `{expression}`\nResult: {result}",
                    Context
                )
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #region Leaderboards

        [Command("leaderboard"), Summary("Get the leaderboard for either \"money\" or \"levels\" globally or locally")]
        [Alias("lb")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetLeaderboard(string type, bool global = false)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();
            var dbguild = await database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);
            switch (type.ToLowerInvariant())
            {
                case "money":
                case "credits":
                    {
                        if (global)
                        {
                            await EmbedExtensions.FromInfo($"View the global money leaderboard at: {SkuldAppContext.LeaderboardMoney}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await EmbedExtensions.FromInfo($"View this server's money leaderboard at: {SkuldAppContext.LeaderboardMoney}/{Context.Guild.Id}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                case "experience":
                case "levels":
                    {
                        if (!database.Features.FirstOrDefault(x => x.Id == dbguild.Id).Experience && !global)
                        {
                            await EmbedExtensions.FromError($"Guild not opted into Experience module. Use: `{dbguild.Prefix}guild-feature levels 1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            return;
                        }

                        if (global)
                        {
                            await EmbedExtensions.FromInfo($"View the global experience leaderboard at: {SkuldAppContext.LeaderboardExperience}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await EmbedExtensions.FromInfo($"View this server's experience leaderboard at: {SkuldAppContext.LeaderboardExperience}/{Context.Guild.Id}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                default:
                    await EmbedExtensions.FromError($"Unknown argument: {type}\n\nMaybe you were looking for: \"levels\", \"experience\", \"money\", \"credits\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    break;
            }
        }

        [Command("commandusage"), Summary("Get the usage for the command specified or all")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task GetCommandUsage([Remainder]string command)
        {
            bool existsButNoData = false;
            CommandLeaderboardInfo info = null;
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();
                if (Database.CustomCommands.Any(x => x.Name == command))
                {
                    var first = Database.CustomCommands.FirstOrDefault(x => x.Name == command);
                    var usage = Database.UserCommandUsage.FirstOrDefault(x => x.UserId == Context.User.Id && x.Command == command);
                    var ranking = await Database.UserCommandUsage.AsAsyncEnumerable().Where(x => x.Command == command).OrderByDescending(x => x.Usage).ToListAsync();

                    if (first != null && usage != null && ranking.Any())
                    {
                        info = new CommandLeaderboardInfo
                        {
                            Name = first.Name,
                            Usage = usage.Usage,
                            Rank = (ulong)ranking.IndexOf(ranking.FirstOrDefault(x => x.UserId == Context.User.Id)) + 1,
                            Total = (ulong)ranking.Count
                        };
                    }
                    else if (first != null)
                    {
                        existsButNoData = true;
                    }
                }
            }

            if (info == null)
            {
                var result = BotService.CommandService.Search(command);

                if (result.IsSuccess)
                {
                    using var Database = new SkuldDbContextFactory().CreateDbContext();
                    var usage = Database.UserCommandUsage.FirstOrDefault(x => x.UserId == Context.User.Id && x.Command == command);
                    var ranking = await Database.UserCommandUsage.AsAsyncEnumerable().Where(x => x.Command == command).OrderByDescending(x => x.Usage).ToListAsync();

                    if (usage != null && ranking.Any())
                    {
                        info = new CommandLeaderboardInfo
                        {
                            Name = result.Commands.FirstOrDefault().Command.Name,
                            Usage = usage.Usage,
                            Rank = (ulong)ranking.IndexOf(ranking.FirstOrDefault(x => x.UserId == Context.User.Id)) + 1,
                            Total = (ulong)ranking.Count
                        };
                    }
                    else
                    {
                        existsButNoData = true;
                    }
                }
            }

            if (info == null && !existsButNoData)
            {
                await
                    EmbedExtensions.FromError($"Couldn't find a command like: `{command}`. Please verify input and try again.", Context)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            else if (existsButNoData)
            {
                await
                    EmbedExtensions.FromError($"You haven't used the command: `{command}`. Please use it and try again", Context)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            await
                new EmbedBuilder()
                .AddAuthor(Context.Client)
                .AddFooter(Context)
                .AddInlineField("Command", info.Name)
                .AddInlineField("Usage", info.Usage)
                .AddInlineField("Rank", $"{info.Rank}/{info.Total}")
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        private class CommandLeaderboardInfo
        {
            public string Name;
            public ulong Usage;
            public ulong Rank;
            public ulong Total;
        }

        #endregion Leaderboards

        #region Time

        [Command("time"), Summary("Gets current time")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Time()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var sUser = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
            var time = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(sUser.TimeZone)).ToDateTimeUnspecified().ToDMYString();

            if (sUser.TimeZone != null)
            {
                await $"Your time is currently; {time}".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await $"UTC Time is currently: {DateTime.UtcNow.ToDMYString()}".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("time"), Summary("Converts a time to a set of times")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task ConvertTime(params IGuildUser[] users)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var message = new StringBuilder("```cs\n");

            foreach (var user in users)
            {
                try
                {
                    var sUser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

                    if (sUser.TimeZone != null)
                    {
                        message.Append("\"The time for ");
                        message.Append(user.Username);
                        message.Append(" is: ");
                        message.Append($"{Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(sUser.TimeZone)).ToDateTimeUnspecified().ToDMYString()}\"\n");
                    }
                    else
                    {
                        message.Append("\"");
                        message.Append(sUser.Username);
                        message.Append(" has not given me their timezone\"\n");
                    }
                }
                catch
                {
                }
            }

            message.Append("```");

            await message.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion Time

        #region IAmRole

        [Command("addrole"), Summary("Adds yourself to a role")]
        [Alias("iam"), RequireDatabase]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task IamRole(int page = 0, [Remainder]IRole role = null)
        {
            if (page != 0)
                page -= 1;
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Context.IsPrivate)
            {
                await EmbedExtensions.FromError("DM's are not supported for this command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var iamlist = await Database.IAmRoles.ToAsyncEnumerable().Where(x => x.GuildId == Context.Guild.Id).ToListAsync();

            if (iamlist.Any())
            {
                if (role == null || !iamlist.Any(x => x.RoleId == role.Id))
                {
                    var paged = iamlist.Paginate(await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild, Context.Guild.GetUser(Context.User.Id));

                    if (page >= paged.Count)
                    {
                        await EmbedExtensions.FromError($"There are only {paged.Count} pages to scroll through", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    await EmbedExtensions.FromMessage($"Joinable roles of __{Context.Guild.Name}__ {page + 1}/{paged.Count}", paged[page], Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await EmbedExtensions.FromError($"{Context.Guild.Name} has no joinable roles", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("addrole"), Summary("Adds yourself to a role")]
        [Alias("iam"), RequireDatabase]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task IamRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Context.IsPrivate)
            {
                await EmbedExtensions.FromError("DM's are not supported for this command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var iamlist = await Database.IAmRoles.ToAsyncEnumerable().Where(x => x.GuildId == Context.Guild.Id).ToListAsync();

            if (iamlist.Any())
            {
                var r = iamlist.FirstOrDefault(x => x.RoleId == role.Id);

                if (r != null)
                {
                    var didpass = CheckIAmValidAsync(await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false), Context.User as IGuildUser, await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild, r);

                    if (didpass != IAmFail.Success)
                    {
                        await EmbedExtensions.FromError(GetErrorIAmFail(didpass, r, await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild), Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        if ((Context.User as IGuildUser).RoleIds.Any(x => x == r.RoleId))
                        {
                            await EmbedExtensions.FromError("You already have that role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            return;
                        }

                        try
                        {
                            var ro = Context.Guild.GetRole(r.RoleId);
                            await (Context.User as IGuildUser).AddRoleAsync(ro).ConfigureAwait(false);
                            await EmbedExtensions.FromSuccess($"You now have the role \"{ro.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                            if (r.Price > 0)
                            {
                                var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
                                usr.Money = usr.Money.Subtract(r.Price);

                                await Database.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("403"))
                            {
                                await EmbedExtensions.FromError("I need to be above the role and have the permission `MANAGE_ROLES` in order to give the role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            }
                            else
                            {
                                await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            }
                            Log.Error(SkuldAppContext.GetCaller(), ex.Message, ex);
                        }
                    }
                }
                else
                {
                    await EmbedExtensions.FromError("Role isn\'t configured to be self assignable", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await EmbedExtensions.FromInfo($"{Context.Guild.Name} has no joinable roles", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("removerole"), Summary("Removes yourself from a role")]
        [Alias("iamnot"), RequireDatabase]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task IamNotRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var g = Context.User as IGuildUser;

            var iamlist = await Database.IAmRoles.ToAsyncEnumerable().Where(x => x.GuildId == Context.Guild.Id).ToListAsync();

            if (iamlist.Any(x => x.RoleId == role.Id))
            {
                if (g.RoleIds.Contains(role.Id))
                {
                    try
                    {
                        await g.RemoveRoleAsync(role).ConfigureAwait(false);
                        await EmbedExtensions.FromSuccess($"You are no longer \"{role.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("403"))
                        {
                            await EmbedExtensions.FromError($"Ensure that I have `MANAGE_ROLES` and that I am above the role \"{role.Name}\" in order to remove it", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        Log.Error(SkuldAppContext.GetCaller(), ex.Message, ex);
                    }
                }
                else
                {
                    await EmbedExtensions.FromInfo("You don\'t have that role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await EmbedExtensions.FromError("Role isn\'t configured to be self assignable", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        private string GetErrorIAmFail(IAmFail amFail, IAmRole role, Guild sguild, IGuild guild)
            => amFail switch
            {
                IAmFail.Price => $"You don\'t have enough money. You need at least {sguild.MoneyIcon}{role.Price}",
                IAmFail.Level => $"You don\'t have the level required for this role (Level: {role.LevelRequired})",
                IAmFail.RequiredRole => $"You don\'t have the required role for this role. You need the role \"{guild.GetRole(role.RequiredRoleId).Name}\"",
                _ => "",
            };

        private IAmFail CheckIAmValidAsync(User suser, IGuildUser user, Guild sguild, IGuild guild, IAmRole roleconf)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (roleconf.RequiredRoleId != 0)
            {
                if (!user.RoleIds.Any(x => x == roleconf.RequiredRoleId))
                {
                    return IAmFail.RequiredRole;
                }
            }

            if (suser.Money < (ulong)roleconf.Price)
            {
                return IAmFail.Price;
            }

            var guildExperience = Database.UserXp.FirstOrDefault(x => x.UserId == suser.Id && x.GuildId == guild.Id);

            if (guildExperience != null)
            {
                if (guildExperience.Level < (ulong)roleconf.LevelRequired && Database.Features.FirstOrDefault(x => x.Id == sguild.Id).Experience)
                {
                    return IAmFail.Level;
                }
            }
            else if (roleconf.LevelRequired != 0)
            {
                return IAmFail.Level;
            }

            return IAmFail.Success;
        }

        #endregion IAmRole
    }
}