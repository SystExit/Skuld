using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using Skuld.APIS;
using Skuld.Bot.Services;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord.Commands;
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
    public class Information : InteractiveBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        public BaseClient WebHandler { get; set; }
        public Locale Locale { get; set; }

        [Command("server"), Summary("Gets information about the server")]
        public async Task GetServer()
        {
            Embed embed = null;
            if (await DatabaseClient.CheckConnectionAsync())
            {
                embed = await Context.Guild.GetSummaryAsync(Context.Client, Context.DBGuild);
            }
            else
            {
                embed = await Context.Guild.GetSummaryAsync(Context.Client);
            }

            await embed.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("server-emojis")]
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
                message = message.Substring(0, message.Length - 2);
            }
            await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("server-roles")]
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
            if (roles.Count() != 0)
            { message += "`" + serverroles + "`"; }
            await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("id-guild"), Summary("Get ID of Guild")]
        public async Task GuildID() =>
            await $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("id"), Summary("Gets a users ID")]
        public async Task GetID(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            await $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("id"), Summary("Get id of channel")]
        public async Task ChanID(IChannel channel) =>
            await $"The ID of **{channel.Name}** is `{channel.Id}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("support"), Summary("Gives discord invite")]
        public async Task DevDisc()
        {
            await $"Join the support server at: http://discord.gg/JYzvjah".QueueMessage(Discord.Models.MessageType.DMS, Context.User, Context.Channel);
        }

        [Command("invite"), Summary("OAuth2 Invite")]
        public async Task BotInvite()
            => await $"Invite me using: https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=1073802246".QueueMessage(Discord.Models.MessageType.DMS, Context.User, Context.Channel);

        [Command("userratio"), Summary("Gets the ratio of users to bots")]
        public async Task HumanToBotRatio()
        {
            var guild = Context.Guild as SocketGuild;
            var bots = await guild.RobotMembersAsync();
            var users = await guild.HumanMembersAsync();
            var ratio = await guild.GetBotUserRatioAsync();
            var usercount = guild.Users.Count;
            await $"Current Bots are: {bots}\nCurrent Users are: {users}\nTotal Guild Users: {usercount}\n{ratio}% of the Guild Users are bots".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("avatar"), Summary("Gets your avatar url")]
        public async Task Avatar([Remainder]IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var avatar = user.GetAvatarUrl(ImageFormat.Auto, 512);
            if (avatar.Contains("a_"))
                avatar = user.GetAvatarUrl(ImageFormat.Gif, 512);
            else if (avatar == "" || avatar == null)
                avatar = user.GetDefaultAvatarUrl();

            await new EmbedBuilder
            {
                Description = $"Avatar for {user.Mention}",
                ImageUrl = avatar,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("mods"), Summary("Gives online status of Moderators/Admins")]
        public async Task ModsOnline()
        {
            var guild = Context.Guild as SocketGuild;
            await guild.DownloadUsersAsync();
            string modstatus = "__Moderators__\n";
            string adminstatus = "__Administrators__\n";
            foreach (var user in guild.Users)
            {
                if (user.IsBot) { }
                else
                {
                    if (user.GuildPermissions.Administrator)
                    {
                        if (user.Status == UserStatus.Online) adminstatus += DiscordTools.Online_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle) adminstatus += DiscordTools.Idle_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.DoNotDisturb) adminstatus += DiscordTools.DoNotDisturb_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.Invisible) adminstatus += DiscordTools.Invisible_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.Offline) adminstatus += DiscordTools.Invisible_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                    }
                    else if (user.GuildPermissions.RawValue == DiscordUtilities.ModeratorPermissions.RawValue)
                    {
                        if (user.Status == UserStatus.Online) modstatus += DiscordTools.Online_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle) modstatus += DiscordTools.Idle_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.DoNotDisturb) modstatus += DiscordTools.DoNotDisturb_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.Invisible) modstatus += DiscordTools.Invisible_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                        if (user.Status == UserStatus.Offline) modstatus += DiscordTools.Invisible_Emote + $" {user.Username}#{user.DiscriminatorValue}\n";
                    }
                }
            }
            string message = "";
            if (modstatus != "__Moderators__\n")
            { message = modstatus + "\n" + adminstatus; }
            else
            { message = adminstatus; }
            await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("createinvite", RunMode = RunMode.Async), Summary("Creates a new invite to the guild")]
        public async Task NewInvite(ITextChannel channel, int maxAge = 0, int maxUses = 0, bool permanent = true, bool unique = true)
        {
            IInviteMetadata invite;
            if (maxAge > 0 && maxUses < 0)
            { invite = await channel.CreateInviteAsync(maxAge, null, permanent, unique); }
            else if (maxAge < 0 && maxUses > 0)
            { invite = await channel.CreateInviteAsync(null, maxUses, permanent, unique); }
            else if (maxAge < 0 && maxUses < 0)
            { invite = await channel.CreateInviteAsync(null, null, permanent, unique); }
            else
            { invite = await channel.CreateInviteAsync(maxAge, maxUses, permanent, unique); }
            await ("I created the invite with the following settings:\n" +
                "```cs\n" +
                $"       Channel : {channel.Name}\n" +
                $"   Maximum Age : {maxAge}\n" +
                $"  Maximum Uses : {maxUses}\n" +
                $"     Permanent : {permanent}\n" +
                $"        Unique : {unique}" +
                $"```Here's the link: {invite.Url}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("nick", RunMode = RunMode.Async), Summary("Gets the user behind the nickname")]
        public async Task Nickname([Remainder]IGuildUser user)
        {
            if (!string.IsNullOrEmpty(user.Nickname))
            { await $"The user **{user.Username}#{user.DiscriminatorValue}** has a nickname of: {user.Nickname}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel); }
            else
            { await "This user doesn't have a nickname set.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel); }
        }

        [Command("me")]
        public async Task Whois()
            => await GetProile(Context.User as IGuildUser).ConfigureAwait(false);

        [Command("whois"), Summary("Get's information about a user"), Alias("user")]
        public async Task GetProile([Remainder]IGuildUser whois = null)
        {
            if (whois == null)
                whois = (IGuildUser)Context.User;
            try
            {
                await whois.GetWhois(EmbedUtils.RandomColor(), Context.Client, Configuration).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Cmd", "Error Encountered Parsing Whois", LogSeverity.Error, ex));
            }
        }

        [Command("roles"), Summary("Gets a users current roles")]
        public async Task GetRole(IGuildUser user = null)
        {
            if (user == null)
                user = (IGuildUser)Context.User;

            var guild = Context.Guild;
            var userroles = user.RoleIds;
            var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            await $"Roles of __**{user.Username}#{user.Discriminator} ({user.Nickname})**__ ({userroles.Count})\n\n`{(roles ?? "No roles")}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("leaderboard"), Summary("Get the guilds XP Leaderboard, use \"money\" for the global money leaderboard")]
        [Alias("lb")]
        public async Task GetLeaderboard(string type)
        {
            switch (type.ToLower())
            {
                case "money":
                case "credits":
                    {
                        var moneylbresp = await DatabaseClient.GetMoneyLeaderboardAsync();
                        if (moneylbresp.Successful)
                        {
                            var moneylb = moneylbresp.Data as IList<MoneyLeaderboardEntry>;

                            var pages = moneylb.PaginateLeaderboard(Configuration);

                            if (pages.Count > 1)
                            {
                                await PagedReplyAsync(new PaginatedMessage
                                {
                                    Pages = pages,
                                    Title = "Global Money Leaderboard",
                                    Color = Color.Purple
                                });
                            }
                            else
                            {
                                await $"{string.Join("\n", pages)}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                            }
                            return;
                        }
                        else
                        {
                            await moneylbresp.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                            return;
                        }
                    }

                case "experience":
                case "level":
                case "levels":
                    {
                        if (Context.DBGuild.Features.Experience)
                        {
                            var guildCountResp = await DatabaseClient.GetGuildExperienceCountAsync(Context.Guild.Id).ConfigureAwait(false);
                            int guildCount = -1;
                            if (guildCountResp.Successful && guildCountResp.Data != null)
                                guildCount = ConversionTools.ParseInt32OrDefault(Convert.ToString(guildCountResp.Data));
                            else
                            {
                                await $"Guild not opted into Experience module. Use: `{Context.DBGuild.Prefix}guild-feature levels 1`".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                                return;
                            }

                            var gldxpresp = await DatabaseClient.GetGuildExperienceAsync(Context.Guild.Id);
                            if (gldxpresp.Successful && gldxpresp.Data != null)
                            {
                                var gldlb = gldxpresp.Data as IList<ExperienceLeaderboardEntry>;

                                var pages = gldlb.PaginateLeaderboard();

                                if (pages.Count > 1)
                                {
                                    await PagedReplyAsync(new PaginatedMessage
                                    {
                                        Pages = pages,
                                        Title = $"__Experience of {Context.Guild.Name}__",
                                        Color = Color.Purple
                                    });
                                }
                                else
                                {
                                    await $"__Experience of {Context.Guild.Name}__:\n{string.Join("\n", pages)}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                                }
                                return;
                            }
                        }
                        await $"Guild not opted into Experience module. Use: `{Context.DBGuild.Prefix}guild-feature levels 1`".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                        break;
                    }

                default:
                    await $"Unknown argument: {type}".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    return;
            }
        }

        [Command("ipping"), Summary("Pings a specific IP address or domain name")]
        public async Task PingDomain(string url)
        {
            var adds = System.Net.Dns.GetHostAddresses(url);

            if(adds.Length > 0)
                await PingIP(adds[0]);
            else
            {
                await $"Couldn't find host at {url}".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
        }

        [Command("ipping"), Summary("Pings a specific IP address or domain name")]
        public async Task PingIP(System.Net.IPAddress ipAddress)
        {
            await "<:blobok:350673783482351626> Ok. Please standby for the ping results".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel);
            Ping pingboi = new Ping();
            var pings = new List<PingReply>();

            for(int x = 0; x < 4; x++)
            {
                pings.Add(await pingboi.SendPingAsync(ipAddress));
            }

            StringBuilder sb = new StringBuilder();

            int count = 1;
            foreach(var res in pings)
            {
                sb.AppendLine($"{count}. {res.Address} {res.RoundtripTime}ms {res.Status}");
                count++;
            }

            await $"```\n{sb.ToString()}```".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("epoch"), Summary("Gets a DateTime DD/MM/YYYY HH:MM:SS (24 Hour) or the current time in POSIX/Unix Epoch time")]
        public async Task Epoch([Remainder]string epoch = null)
        {
            if (epoch == null)
            {
                var dtnowutc = DateTime.UtcNow;
                await $"The current time in UTC ({dtnowutc.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {(Int32)(dtnowutc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                var datetime = Convert.ToDateTime(epoch, new CultureInfo("en-GB"));
                var epochdt = (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                await $"Your DateTime ({datetime.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {epochdt}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("epoch"), Summary("epoch to DateTime format")]
        public async Task Epoch(ulong epoch)
        {
            var epochtodt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(epoch));
            await $"Your epoch ({epoch}) in DateTime is: {epochtodt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("isup"), Summary("Check if a website is online"), Alias("downforeveryone", "isitonline")]
        public async Task IsUp(Uri website)
        {
            var res = await WebHandler.ScrapeUrlAsync(website);

            if (res != null)
            { await $"The website: `{website}` is working and replying as intended.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel); }
            else
            { await $"The website: `{website}` is down or not replying.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel); }
        }

        [Command("time"), Summary("Converts a time to a set of times")]
        public async Task ConvertTime(string primarytimezone, string time, [Remainder]string timezones)
        {
            try
            {
                var usertimes = timezones.Split(' ');
                string response = $"The requested time of: {primarytimezone} - {time} is:```";
                var primaryDateTime = Convert.ToDateTime(time);

                foreach (var usertime in usertimes)
                {
                    var convertedTime = ConvertDateTimeToDifferentTimeZone(primaryDateTime, primarytimezone, usertime);
                    response += $"\n{usertime} - {convertedTime}";
                }

                await (response + "```").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
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
    }
}