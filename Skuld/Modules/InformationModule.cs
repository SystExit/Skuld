using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Discord.WebSocket;
using System.Globalization;
using Skuld.APIS;
using NodaTime;
using Skuld.Services;

namespace Skuld.Modules
{
    [Group, Name("Information")]
    public class Information : ModuleBase<ShardedCommandContext>
    {
		readonly LoggingService logger;
		readonly MessageService messageService;

		public Information(LoggingService log,
			MessageService msg)
		{
			logger = log;
			messageService = msg;
		}

        string streamingemote = "<:streaming:313956277132853248>";
        string onlineemote = "<:online:313956277808005120>";
        string idleemote = "<:away:313956277220802560>";
        string dontdisturbemote = "<:dnd:313956276893646850>";
        string invisibleemote = "<:invisible:313956277107556352>";
        string offlineemote = "<:offline:313956277237710868>";            

        [Command("server"), Summary("Gets information about the server")]
        public async Task GetServer()
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var embed = new EmbedBuilder { Color = Tools.Tools.RandomColor() };
            if (!String.IsNullOrEmpty(guild.IconUrl))
            { embed.ThumbnailUrl = guild.IconUrl; }
            embed.Author = new EmbedAuthorBuilder { Name = guild.Name };
            var users = guild.Users;
            int usercount = users.Count(x => x.IsBot == false);
            var gusersnobot = users.Where(x => x.IsBot == false);
            var gusersbotonly = users.Where(x => x.IsBot == true);
            var owner = guild.Owner;
            string afkname = null;
            if (guild.AFKChannel != null)
            { afkname = guild.AFKChannel.Name; }
            embed.AddField("Users", usercount.ToString(),inline: true);
            embed.AddField("Bots", $"{gusersbotonly.Count()}", inline: true);
            embed.AddField("Shard", Context.Client.GetShardIdFor(guild).ToString(), inline: true);
            embed.AddField("Verification Level", guild.VerificationLevel.ToString(),inline: true);
            embed.AddField("Voice Region ID", guild.VoiceRegionId, inline: true);
            embed.AddField("Owner", owner.Nickname??owner.Username + "#" + owner.DiscriminatorValue,inline: true);
            embed.AddField("Text Channels", (guild.Channels.Count(x=>x.GetType() == typeof(ITextChannel)))+" channels",inline: true);
            embed.AddField("Voice Channels", (guild.Channels.Count(x=>x.GetType() == typeof(IVoiceChannel)))+ " channels",inline: true);
            int seconds = guild.AFKTimeout;
            string minutes = ((seconds % 3600) / 60).ToString();
            embed.AddField("AFK Timeout", minutes + " minutes",inline: true);
            embed.AddField("AFK Channel",afkname??"None Set",inline: true);
            if (String.IsNullOrEmpty(guild.IconUrl))
            { embed.AddField("Server Avatar", "Doesn't exist", inline: true); }
            embed.AddField("Default Notifications", guild.DefaultMessageNotifications.ToString(),inline: true);
            embed.AddField("Created", guild.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            embed.AddField("Emojis", guild.Emotes.Count + Environment.NewLine + $"Use `{messageService.config.Prefix}server emojis` to view all of the emojis");
            embed.AddField("Roles", guild.Roles.Count() + Environment.NewLine + $"Use `{messageService.config.Prefix}server roles` to view all of the roles");
            await messageService.SendChannelAsync((ITextChannel)Context.Channel,"", embed.Build());
        }
        [Command("server emojis"), Alias("server emoji")]
        public async Task ServerEmoji()
        {
            var guild = Context.Guild;
            string message = null;
            var num = 0;
            message += $"Emojis of __**{guild.Name}**__" + Environment.NewLine;
            if (guild.Emotes.Count == 0)
            { message += "Server contains no emotes"; }
            else
            {
                foreach (var emoji in guild.Emotes)
                {
                    num++;
                    if (emoji.Id == guild.Emotes.Last().Id)
                    { message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}>"; }
                    else
                    {
                        if (num % 5 != 0)
                            message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}> | ";
                        else
                            message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}>\n";
                    }
                }
            }
            await messageService.SendChannelAsync(Context.Channel,message);
        }
        [Command("server roles"), Alias("server role")]
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
            message += $"Roles of __**{guild.Name}**__" + Environment.NewLine;
            if (roles.Count == 0)
            { message += "Server contains no roles"; }
            else
            { message += "`" + serverroles + "`"; }
            await messageService.SendChannelAsync(Context.Channel,message);
        }

        [Command("bot")]
        public async Task DInfo() =>
            await messageService.SendDMsAsync((ITextChannel)Context.Channel, await Context.User.GetOrCreateDMChannelAsync(), $"Hey. I'm Skuld, a utility bot that aims to bring tools and fun stuff to your server! :blush:\n\nThanks for using the bot. It means a lot. <3\n\nP.S. I also have a twitter where you can also get support from: <https://twitter.com/SkuldDiscordBot> \n\nOr there's the Support server: http://discord.gg/JYzvjah \n\n" +
                "P.S. from the Dev:\n" +
                "By using this command and all others, past-present-future, you consent to having your User Data be stored for proper functionality of the commands. This also applies to the server information if you are a server admin/moderator");
		        
        [Command("id guild"), Summary("Get ID of Guild")]
        public async Task GuildID() => 
			await messageService.SendChannelAsync(Context.Channel, $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`");

		[Command("id"), Summary("Gets a users ID")]
		public async Task GetID(IUser user = null)
		{
			if (user == null)
				user = Context.User;
			await messageService.SendChannelAsync(Context.Channel, $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`");
		}
        
        [Command("id"), Summary("Get id of channel")]
        public async Task ChanID(IChannel channel) => 
			await messageService.SendChannelAsync(Context.Channel, $"The ID of **{channel.Name}** is `{channel.Id}`");        

        [Command("support"), Summary("Gives discord invite")]
        public async Task DevDisc()
        {
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await messageService.SendDMsAsync((ITextChannel)Context.Channel, user, $"Join the support server at: http://discord.gg/JYzvjah");
        }
        [Command("invite"), Summary("OAuth2 Invite")]
        public async Task BotInvite()
        {
            var bot = await Context.Client.GetApplicationInfoAsync();
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await messageService.SendDMsAsync((ITextChannel)Context.Channel, user, $"Invite me using: https://discordapp.com/oauth2/authorize?client_id={bot.Id}&scope=bot&permissions=1073802246");
        }
        [Command("userratio"), Summary("Gets the ratio of users to bots")]
        public async Task HumanToBotRatio()
        {
            var guild = Context.Guild as SocketGuild;
            await guild.DownloadUsersAsync();
            int bots = guild.Users.Count(x => x.IsBot == true);
            int humans = guild.Users.Count(x=>x.IsBot == false);
            int guildusers = guild.Users.Count;
            var oldperc = (decimal)bots / guildusers*100m;
            var total = Math.Round(oldperc, 2);
            await messageService.SendChannelAsync(Context.Channel, $"Current Bots are: {bots}\nCurrent Users are: {humans}\nTotal Guild Users: {guildusers}\n{total}% of the Guild Users are bots");
        }
		[Command("avatar"), Summary("Gets your avatar url")]
		public async Task Avatar([Remainder]IUser user = null)
		{
			if (user == null)
				user = Context.User;
			await messageService.SendChannelAsync(Context.Channel, user.GetAvatarUrl(ImageFormat.Auto) ?? $"User {user.Username} has no avatar set");
		}

        [Command("mods"),Summary("Gives online status of Moderators/Admins")]
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
                        if (user.Status == UserStatus.Online)
                        { adminstatus += onlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle)
                        { adminstatus += idleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.DoNotDisturb)
                        { adminstatus += dontdisturbemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.Invisible)
                        { adminstatus += invisibleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.Offline)
                        { adminstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                    }
                    else if (user.GuildPermissions.KickMembers && user.GuildPermissions.ManageMessages)
                    {
                        if (user.Status == UserStatus.Online)
                        { modstatus += onlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle)
                        { modstatus += idleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.DoNotDisturb)
                        { modstatus += dontdisturbemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.Invisible)
                        { modstatus += invisibleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                        if (user.Status == UserStatus.Offline)
                        { modstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n"; }
                    }
                }
            }
            string message = "";
            if (modstatus != "__Moderators__\n")
            { message = modstatus + "\n" + adminstatus; }
            else
            { message = adminstatus; }
            await messageService.SendChannelAsync(Context.Channel, message);
        }
        [Command("createinvite",RunMode = RunMode.Async),Summary("Creates a new invite to the guild")]
        public async Task NewInvite(ITextChannel channel,int maxAge = 0,int maxUses=0, bool permanent=true, bool unique=true)
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
            await messageService.SendChannelAsync(Context.Channel,
                "I created the invite with the following settings:\n" +
                "```cs\n" +
                $"       Channel : {channel.Name}\n"+
                $"   Maximum Age : {maxAge}\n" +
                $"  Maximum Uses : {maxUses}\n" +
                $"     Permanent : {permanent}\n" +
                $"        Unique : {unique}" +
                $"```Here's the link: {invite.Url}");
        }
        [Command("nick",RunMode =RunMode.Async),Summary("Gets the user behind the nickname")]
        public async Task Nickname([Remainder]IGuildUser user)
        {
            if (!String.IsNullOrEmpty(user.Nickname))
            { await messageService.SendChannelAsync(Context.Channel, $"The user **{user.Username}#{user.DiscriminatorValue}** has a nickname of: {user.Nickname}"); }
            else
            { await messageService.SendChannelAsync(Context.Channel, "This user doesn't have a nickname set."); }
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
                string nickname = null;
                string status = "";
                if (!String.IsNullOrEmpty(whois.Nickname))
                { nickname = $"({whois.Nickname})"; }
                if (whois.Status == UserStatus.Online)
                { status = onlineemote + " Online"; }
                if (whois.Status == UserStatus.AFK || whois.Status == UserStatus.Idle)
                { status = idleemote + " AFK/Idle"; }
                if (whois.Status == UserStatus.DoNotDisturb)
                { status = dontdisturbemote + " Do not Disturb/Busy"; }
                if (whois.Status == UserStatus.Invisible)
                { status = invisibleemote + " Invisible"; }
                if (whois.Status == UserStatus.Offline)
                { status = offlineemote + " Offline"; }
                if (whois.Activity != null)
                {
                    if (whois.Activity.Type == ActivityType.Streaming)
                    { status = streamingemote + $" {whois.Activity.Type.ToString()}"; }
                }

                string pngavatar = whois.GetAvatarUrl(ImageFormat.Png);
                string gifavatar = whois.GetAvatarUrl(ImageFormat.Gif);
                string name = whois.Username + $"#{whois.DiscriminatorValue}";
                if (whois.Nickname != null)
                { name += $" {nickname}"; }

                string bigavatar="";
                if (!gifavatar.Contains("a_"))
                {
                    if (!String.IsNullOrEmpty(pngavatar))
                    {
                        bigavatar = pngavatar;
                    }
                }
                else
                { bigavatar = gifavatar; }


                var embed = new EmbedBuilder
                {
                    Color = Tools.Tools.RandomColor(),
                    Author = new EmbedAuthorBuilder { IconUrl = pngavatar ?? "", Name = name },
                    ImageUrl =  bigavatar
                };

                int seencount = 0;
                foreach (var item in Context.Client.Guilds)
                {
                    if (item.GetUser(whois.Id) != null)
                    { seencount++; }
                }
                string game = null;
                if (whois.Activity!=null)
                { game = whois.Activity.Name; }
                else
                { game = "Nothing"; }

                embed.AddField(":id: ID", whois.Id.ToString() ?? "Unknown", true);
                embed.AddField(":vertical_traffic_light:  Status", status ?? "Unknown", true);
                embed.AddField(":video_game: Currently Playing", game, true);
                embed.AddField(":robot: Bot?", whois.IsBot.ToString() ?? "Unknown", true);
                embed.AddField(":eyes: Mutual Servers", $"{seencount} servers", true);
                embed.AddField(":shield: Roles", $"[{whois.RoleIds.Count}] Do `{Bot.Configuration.Discord.Prefix}roles` to see your roles");

                var createdatstring = GetStringfromOffset(whois.CreatedAt, DateTime.UtcNow);
                var joinedatstring = GetStringfromOffset(whois.JoinedAt.Value, DateTime.UtcNow);

                embed.AddField(":inbox_tray: Server Join", whois.JoinedAt.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({joinedatstring})\t`DD/MM/YYYY`");
                embed.AddField(":globe_with_meridians: Discord Join", whois.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({createdatstring})\t`DD/MM/YYYY`");

                await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("Cmd", "Error Encountered Parsing Whois", LogSeverity.Error, ex));
            }
        }
        
        string GetStringfromOffset(DateTimeOffset dateTimeOffset, DateTime dateTime)
        {
            var thing = dateTime - dateTimeOffset;
            string rtnstrng = "";
            var temp = thing.TotalDays - Math.Floor(thing.TotalDays);
            int days = (int)Math.Floor(thing.TotalDays);
            rtnstrng += days + " days ";
            return rtnstrng + "ago";
        }

		[Command("roles"), Summary("Gets a users current roles")]
		public async Task GetRole(IGuildUser user = null)
		{
			if (user == null)
				user = (IGuildUser)Context.User;
			var guild = Context.Guild;
			var userroles = user.RoleIds;
			var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
			await messageService.SendChannelAsync(Context.Channel, $"Roles of __**{user.Username}({user.Nickname})#{user.Discriminator}**__\n\n`" + (roles ?? "No roles") + "`");
        }

        [Command("epoch"), Summary("Gets a DateTime DD/MM/YYYY HH:MM:SS (24 Hour) or the current time in POSIX/Unix Epoch time")]
        public async Task Epoch([Remainder]string epoch = null)
        {
			if(epoch == null)
			{
				var dtnowutc = DateTime.UtcNow;
				await messageService.SendChannelAsync(Context.Channel, $"The current time in UTC ({dtnowutc.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {(Int32)(dtnowutc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}");
			}
			else
			{
				var datetime = Convert.ToDateTime(epoch, new CultureInfo("en-GB"));
				var epochdt = (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
				await messageService.SendChannelAsync(Context.Channel, $"Your DateTime ({datetime.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {epochdt}");
			}
        }
        [Command("epoch"), Summary("epoch to DateTime format")]
        public async Task Epoch(ulong epoch)
        {
            var epochtodt = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc).AddSeconds(Convert.ToDouble(epoch));
            await messageService.SendChannelAsync(Context.Channel, $"Your epoch ({epoch}) in DateTime is: {epochtodt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}");
        }

        [Command("isup"), Summary("Check if a website is online"), Alias("downforeveryone", "isitonline")]
        public async Task IsUp(string website)
        {
            var doc = await WebHandler.ScrapeUrlAsync(new Uri("http://downforeveryoneorjustme.com/" + website));
            string response = null;
            var container = doc.GetElementbyId("domain-main-content");
            var isup = container.ChildNodes.FindFirst("p");
            if (isup.InnerHtml.ToLowerInvariant().Contains("not"))
            { response = $"The website: `{website}` is down or not replying."; }
            else
            { response = $"The website: `{website}` is working and replying as intended."; }
            response = response + "\n\n`Source:` <http://downforeveryoneorjustme.com/" + website + ">";
            await messageService.SendChannelAsync(Context.Channel, response);
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

                await messageService.SendChannelAsync(Context.Channel, response + "```");
            }
            catch (Exception ex)
            {
                await messageService.SendChannelAsync(Context.Channel, Skuld.Languages.en_GB.ResourceManager.GetString("SKULD_GENERIC_ERROR") + "\n" + ex.Message);
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
