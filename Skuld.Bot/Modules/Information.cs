using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using Skuld.APIS;
using Skuld.Bot.Extensions;
using Skuld.Bot.Globalization;
using Skuld.Bot.Services;
using Skuld.Core;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Information : ModuleBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        public BaseClient WebHandler { get; set; }
        public Locale Locale { get; set; }

        [Command("server"), Summary("Gets information about the server")]
        public async Task GetServer()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var dbguild = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            Embed embed = await Context.Guild.GetSummaryAsync(Context.Client, dbguild).ConfigureAwait(false);

            await embed.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("server-emojis"), RequireContext(ContextType.Guild)]
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
        public async Task ServerRoles()
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            string serverroles = null;
            foreach (var item in roles)
            {
                string thing = item.Name.TrimStart('@');
                if (item == guild.Roles.Last())
                { serverroles += thing; }
                else
                { serverroles += thing + ", "; }
            }
            string message = null;
            message += $"Roles of __**{guild.Name}**__ ({roles.Count()})\n" + Environment.NewLine;
            if (roles.Any())
            { message += "`" + serverroles + "`"; }
            await message.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("id-guild"), Summary("Get ID of Guild"), RequireContext(ContextType.Guild)]
        public async Task GuildID() =>
            await $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`"
                .QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("id"), Summary("Gets a users ID")]
        public async Task GetID(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            await $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`"
                .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("id"), Summary("Get id of channel"), RequireContext(ContextType.Guild)]
        public async Task ChanID(IChannel channel) =>
            await $"The ID of **{channel?.Name}** is `{channel.Id}`".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("screenshare"), Summary("Get's the screenshare channel link"), RequireContext(ContextType.Guild), RequireGuildVoiceChannel]
        [Alias("sc")]
        public async Task Screenshare()
            => await $"https://discordapp.com/channels/{Context.Guild.Id}/{(Context.User as IGuildUser)?.VoiceChannel.Id}".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("support"), Summary("Gives discord invite")]
        public async Task DevDisc()
            => await $"Join the support server at: https://discord.skuldbot.uk/discord?ref=bot"
            .QueueMessageAsync(Context, Discord.Models.MessageType.DMS).ConfigureAwait(false);

        [Command("invite"), Summary("OAuth2 Invite")]
        public async Task BotInvite()
            => await $"Invite me using: https://discord.skuldbot.uk/bot?ref=bot".QueueMessageAsync(Context, Discord.Models.MessageType.DMS).ConfigureAwait(false);

        [Command("userratio"), Summary("Gets the ratio of users to bots")]
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
        public async Task Avatar([Remainder]IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var avatar = user.GetAvatarUrl(ImageFormat.Auto, 512);
            if (avatar.Contains("a_"))
                avatar = user.GetAvatarUrl(ImageFormat.Gif, 512);
            else switch (avatar)
                {
                    case "":
                    case null:
                        avatar = user.GetDefaultAvatarUrl();
                        break;
                }

            await new EmbedBuilder
            {
                Description = $"Avatar for {user.Mention}",
                ImageUrl = avatar,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("mods"), Summary("Gives online status of Moderators/Admins")]
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
                                admins.Add(DiscordTools.Streaming_Emote + $" {user.FullNameWithNickname()}");
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
                                mods.Add(DiscordTools.Streaming_Emote + $" {user.FullNameWithNickname()}");
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
        public async Task Whois()
        {
            if (!Context.IsPrivate)
            {
                await GetProileAsync(Context.User as IGuildUser).ConfigureAwait(false);
                return;
            }
            else
            {
                await Context.User.GetWhois(null, null, EmbedUtils.RandomColor(), Context.Client, Configuration).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
        }

        [Command("whois"), Summary("Get's information about a user"), Alias("user")]
        public async Task GetProileAsync([Remainder]IGuildUser whois = null)
        {
            Color color = Color.Default;

            if (!Context.IsPrivate)
            {
                if (whois == null)
                    whois = (IGuildUser)Context.User;

                color = whois.GetHighestRoleColor(Context.Guild);
            }

            color = color == Color.Default ? EmbedUtils.RandomColor() : color;

            await whois.GetWhois(whois, whois.RoleIds, color, Context.Client, Configuration).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("roles"), Summary("Gets a users current roles")]
        public async Task GetRole(IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser)Context.User;

            var guild = Context.Guild;
            var userroles = user.RoleIds;
            var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            await $"Roles of __**{user.Username}#{user.Discriminator} ({user.Nickname})**__ ({userroles.Count})\n\n`{(roles ?? "No roles")}`".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("leaderboard"), Summary("Get the leaderboard for either \"money\" or \"levels\" globally or locally")]
        [Alias("lb")]
        public async Task GetLeaderboard(string type, bool global = false)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();
            var dbguild = await database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            switch (type.ToLowerInvariant())
            {
                case "money":
                case "credits":
                    {
                        if(global)
                        {
                            await Messages.FromInfo($"View the global money leaderboard at: {SkuldAppContext.LeaderboardMoney}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await Messages.FromInfo($"View this server's money leaderboard at: {SkuldAppContext.LeaderboardMoney}/{Context.Guild.Id}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                case "experience":
                case "levels":
                    {
                        if (!database.Features.FirstOrDefault(x => x.Id == dbguild.Id).Experience && !global)
                        {
                            await Messages.FromError($"Guild not opted into Experience module. Use: `{dbguild.Prefix}guild-feature levels 1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            return;
                        }

                        if(global)
                        {
                            await Messages.FromInfo($"View the global experience leaderboard at: {SkuldAppContext.LeaderboardExperience}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await Messages.FromInfo($"View this server's experience leaderboard at: {SkuldAppContext.LeaderboardExperience}/{Context.Guild.Id}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                default:
                    await Messages.FromError($"Unknown argument: {type}\n\nMaybe you were looking for: \"levels\", \"experience\", \"money\", \"credits\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    break;
            }
        }

        [Command("ipping"), Summary("Pings a specific IP address or domain name")]
        public async Task PingDomain(string url)
        {
            var adds = System.Net.Dns.GetHostAddresses(url);

            if (adds.Length > 0)
                await PingIP(adds[0]).ConfigureAwait(false);
            else
            {
                await Messages.FromError($"Couldn't find host at {url}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
        }

        [Command("ipping"), Summary("Pings a specific IP address or domain name")]
        public async Task PingIP(System.Net.IPAddress ipAddress)
        {
            var message = await Messages.FromSuccess("<:blobok:350673783482351626> Ok. Please standby for the ping results", Context).QueueMessageAsync(Context).ConfigureAwait(false);

            var pings = new List<PingReply>();

            using (Ping pingboi = new Ping())
            {
                for (int x = 0; x < 4; x++)
                {
                    pings.Add(await pingboi.SendPingAsync(ipAddress).ConfigureAwait(false));
                }
            }

            StringBuilder sb = new StringBuilder();

            int count = 1;
            foreach (var res in pings)
            {
                sb.AppendLine($"{count}. {res.Address} {res.RoundtripTime}ms {res.Status}");
                count++;
            }

            Embed embed = null;

            if (pings.All(x => x.Status == IPStatus.Success))
                embed = Messages.FromSuccess($"```\n{sb.ToString()}```", Context).Build();
            else if (pings.All(x => x.Status != IPStatus.Success))
                embed = Messages.FromError($"```\n{sb.ToString()}```", Context).Build();
            else
                embed = Messages.FromInfo($"```\n{sb.ToString()}```", Context).Build();

            await message.ModifyAsync(x=>x.Embed = embed).ConfigureAwait(false);
        }

        [Command("isup"), Summary("Check if a website is online"), Alias("downforeveryone", "isitonline")]
        public async Task IsUp(Uri website)
        {
            (var res, _) = await WebHandler.ScrapeUrlAsync(website).ConfigureAwait(false);

            if (res != null)
            {
                await $"The website: `{website}` is working and replying as intended.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await $"The website: `{website}` is down or not replying.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("time"), Summary("Converts a time to a set of times")]
        public async Task ConvertTime(params IGuildUser[] users)
        {
            /*try
            {
                var usertimes = timezones.Split(' ');
                string response = $"The requested time of: {primarytimezone} - {time} is:```";
                DateTime primaryDateTime = Convert.ToDateTime(time);

                foreach (var usertime in usertimes)
                {
                    var convertedTime = ConvertDateTimeToDifferentTimeZone(primaryDateTime, primarytimezone, usertime);
                    response += $"\n{usertime} - {convertedTime}";
                }

                await (response + "```").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessageAsync(Context, Discord.Models.MessageType.Failed, exception: ex).ConfigureAwait(false);
            }*/
        }

        //https://stackoverflow.com/questions/39208477/is-this-the-proper-way-to-convert-between-time-zones-in-nodatime
        public static DateTime ConvertDateTimeToDifferentTimeZone(DateTime fromDateTime, string fromZoneId, string toZoneId)
        {
            var fromLocal = LocalDateTime.FromDateTime(fromDateTime);
            var fromZone = DateTimeZoneProviders.Tzdb[fromZoneId];
            var fromZoned = fromLocal.InZoneLeniently(fromZone);

            var toZone = DateTimeZoneProviders.Tzdb[toZoneId];
            var toZoned = fromZoned.WithZone(toZone);
            var toLocal = toZoned.LocalDateTime;
            return toLocal.ToDateTimeUnspecified();
        }

        [Command("addrole"), Summary("Adds yourself to a role")]
        [Alias("iam"), RequireDatabase]
        public async Task IamRole(int page = 0, [Remainder]IRole role = null)
        {
            if (page != 0)
                page = page - 1;
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Context.IsPrivate)
            {
                await Messages.FromError("DM's are not supported for this command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var iamlist = Database.IAmRoles.Where(x => x.GuildId == Context.Guild.Id).ToList();

            if (iamlist.Any())
            {
                if (role == null || !iamlist.Any(x => x.RoleId == role.Id))
                {
                    var paged = iamlist.Paginate(await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild, Context.Guild.GetUser(Context.User.Id));

                    if(page >= paged.Count)
                    {
                        await Messages.FromError($"There are only {paged.Count} pages to scroll through", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    await Messages.FromMessage($"Joinable roles of __{Context.Guild.Name}__ {page + 1}/{paged.Count}", paged[page], Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await Messages.FromError($"{Context.Guild.Name} has no joinable roles", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("addrole"), Summary("Adds yourself to a role")]
        [Alias("iam"), RequireDatabase]
        public async Task IamRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Context.IsPrivate)
            {
                await Messages.FromError("DM's are not supported for this command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var iamlist = Database.IAmRoles.Where(x => x.GuildId == Context.Guild.Id).ToList();

            if (iamlist.Any())
            {
                var r = iamlist.FirstOrDefault(x => x.RoleId == role.Id);

                var didpass = CheckIAmValidAsync(await Database.GetUserAsync(Context.User).ConfigureAwait(false), Context.User as IGuildUser, await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild, r);

                if (didpass != IAmFail.Success)
                {
                    await Messages.FromError(GetErrorIAmFail(didpass, r, await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false), Context.Guild), Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    if ((Context.User as IGuildUser).RoleIds.Any(x => x == r.RoleId))
                    {
                        await Messages.FromError("You already have that role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    try
                    {
                        var ro = Context.Guild.GetRole(r.RoleId);
                        await (Context.User as IGuildUser).AddRoleAsync(ro).ConfigureAwait(false);
                        await Messages.FromSuccess($"You now have the role \"{ro.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                        if (r.Price > 0)
                        {
                            (await Database.GetUserAsync(Context.User).ConfigureAwait(false)).Money -= (ulong)r.Price;

                            await Database.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("403"))
                        {
                            await Messages.FromError("I need to be above the role as well as have `MANAGE_ROLES` in order to give the role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await Messages.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        Log.Error(Utils.GetCaller(), ex.Message, ex);
                    }
                }
            }
            else
            {
                await Messages.FromInfo($"{Context.Guild.Name} has no joinable roles", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("removerole"), Summary("Removes yourself from a role")]
        [Alias("iamnot"), RequireDatabase]
        public async Task IamNotRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var g = Context.User as IGuildUser;
            var iamlist = Database.IAmRoles.Where(x => x.GuildId == Context.Guild.Id);

            if (iamlist.Any(x => x.RoleId == role.Id))
            {
                try
                {
                    await g.RemoveRoleAsync(role).ConfigureAwait(false);
                    await Messages.FromSuccess($"You are no longer \"{role.Name}\"", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("403"))
                    {
                        await Messages.FromError($"Ensure that I have `MANAGE_ROLES` and that I am above the role \"{role.Name}\" in order to remove it", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await Messages.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    Log.Error(Utils.GetCaller(), ex.Message, ex);
                }
            }
            else
            {
                await Messages.FromInfo("You already don\'t have that role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
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
    }
}