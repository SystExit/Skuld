using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord;
using Skuld.Discord.Extensions;
using Skuld.Discord.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Information : SkuldBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
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

            await ReplyAsync(Context.Channel, embed);
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
            await ReplyAsync(Context.Channel, message);
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
            message += $"Roles of __**{guild.Name}**__ ({roles.Count})\n" + Environment.NewLine;
            if (roles.Count != 0)
            { message += "`" + serverroles + "`"; }
            await ReplyAsync(Context.Channel, message);
        }

        [Command("id-guild"), Summary("Get ID of Guild")]
        public async Task GuildID() =>
            await ReplyAsync(Context.Channel, $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`");

        [Command("id"), Summary("Gets a users ID")]
        public async Task GetID(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            await ReplyAsync(Context.Channel, $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`");
        }

        [Command("id"), Summary("Get id of channel")]
        public async Task ChanID(IChannel channel) =>
            await ReplyAsync(Context.Channel, $"The ID of **{channel.Name}** is `{channel.Id}`");

        [Command("support"), Summary("Gives discord invite")]
        public async Task DevDisc()
        {
            var user = await Context.User.GetOrCreateDMChannelAsync();
            var dmchan = await Context.User.GetOrCreateDMChannelAsync();
            await ReplyAsync(dmchan, Context.Channel, $"Join the support server at: http://discord.gg/JYzvjah");
        }

        [Command("invite"), Summary("OAuth2 Invite")]
        public async Task BotInvite()
        {
            var user = await Context.User.GetOrCreateDMChannelAsync();
            var dmchan = await Context.User.GetOrCreateDMChannelAsync();
            await ReplyAsync(dmchan, Context.Channel, $"Invite me using: https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=1073802246");
        }

        [Command("userratio"), Summary("Gets the ratio of users to bots")]
        public async Task HumanToBotRatio()
        {
            var guild = Context.Guild as SocketGuild;
            var bots = await guild.RobotMembersAsync();
            var users = await guild.HumanMembersAsync();
            var ratio = await guild.GetBotUserRatioAsync();
            var usercount = guild.Users.Count;
            await ReplyAsync(Context.Channel, $"Current Bots are: {bots}\nCurrent Users are: {users}\nTotal Guild Users: {usercount}\n{ratio}% of the Guild Users are bots");
        }

        [Command("avatar"), Summary("Gets your avatar url")]
        public async Task Avatar([Remainder]IUser user = null)
        {
            if (user == null) user = Context.User;

            await ReplyAsync(Context.Channel, user.GetAvatarUrl(ImageFormat.Auto) ?? $"User {user.Username} has no avatar set");
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
            await ReplyAsync(Context.Channel, message);
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
            await ReplyAsync(Context.Channel,
                "I created the invite with the following settings:\n" +
                "```cs\n" +
                $"       Channel : {channel.Name}\n" +
                $"   Maximum Age : {maxAge}\n" +
                $"  Maximum Uses : {maxUses}\n" +
                $"     Permanent : {permanent}\n" +
                $"        Unique : {unique}" +
                $"```Here's the link: {invite.Url}");
        }

        [Command("nick", RunMode = RunMode.Async), Summary("Gets the user behind the nickname")]
        public async Task Nickname([Remainder]IGuildUser user)
        {
            if (!string.IsNullOrEmpty(user.Nickname))
            { await ReplyAsync(Context.Channel, $"The user **{user.Username}#{user.DiscriminatorValue}** has a nickname of: {user.Nickname}"); }
            else
            { await ReplyAsync(Context.Channel, "This user doesn't have a nickname set."); }
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
                await ReplyAsync(Context.Channel, whois.GetWhois(EmbedUtils.RandomColor(), Context.Client, Configuration));
            }
            catch (Exception ex)
            {
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
            await ReplyAsync(Context.Channel, $"Roles of __**{user.Username}#{user.Discriminator} ({user.Nickname})**__ ({userroles.Count})\n\n`" + (roles ?? "No roles") + "`");
        }

        [Command("epoch"), Summary("Gets a DateTime DD/MM/YYYY HH:MM:SS (24 Hour) or the current time in POSIX/Unix Epoch time")]
        public async Task Epoch([Remainder]string epoch = null)
        {
            if (epoch == null)
            {
                var dtnowutc = DateTime.UtcNow;
                await ReplyAsync(Context.Channel, $"The current time in UTC ({dtnowutc.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {(Int32)(dtnowutc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}");
            }
            else
            {
                var datetime = Convert.ToDateTime(epoch, new CultureInfo("en-GB"));
                var epochdt = (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                await ReplyAsync(Context.Channel, $"Your DateTime ({datetime.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {epochdt}");
            }
        }

        [Command("epoch"), Summary("epoch to DateTime format")]
        public async Task Epoch(ulong epoch)
        {
            var epochtodt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(epoch));
            await ReplyAsync(Context.Channel, $"Your epoch ({epoch}) in DateTime is: {epochtodt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}");
        }

        [Command("isup"), Summary("Check if a website is online"), Alias("downforeveryone", "isitonline")]
        public async Task IsUp(Uri website)
        {
            var res = await WebHandler.ScrapeUrlAsync(website);

            if (res != null)
            { await ReplyAsync(Context.Channel, $"The website: `{website}` is working and replying as intended."); }
            else
            { await ReplyAsync(Context.Channel, $"The website: `{website}` is down or not replying."); }
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

                await ReplyAsync(Context.Channel, response + "```");
            }
            catch (Exception ex)
            {
                await ReplyAsync(Context.Channel, Locale.GetLocale(Locale.defaultLocale).GetString("SKULD_GENERIC_ERROR") + "\n" + ex.Message);
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