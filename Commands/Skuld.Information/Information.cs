using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Skuld.Tools;
using Discord.WebSocket;
using System.Globalization;

namespace Skuld.Commands
{
    [Group,Name("Information")]
    public class Information : ModuleBase
    {
        private string streamingemote = "<:streaming:313956277132853248>";
        private string onlineemote = "<:online:313956277808005120>";
        private string idleemote = "<:away:313956277220802560>";
        private string dontdisturbemote = "<:dnd:313956276893646850>";
        private string invisibleemote = "<:invisible:313956277107556352>";
        private string offlineemote = "<:offline:313956277237710868>";

        [Command("server", RunMode = RunMode.Async), Summary("Gets information about the server")]
        public async Task GetServer()
        {
            var bot = Bot.bot;
            var guild = Context.Guild;
            var roles = guild.Roles;
            EmbedBuilder _embed = new EmbedBuilder() { Color = RandColor.RandomColor() };
            if (!String.IsNullOrEmpty(guild.IconUrl))
                _embed.ThumbnailUrl = guild.IconUrl;
            _embed.Author = new EmbedAuthorBuilder() { Name = guild.Name };
            var users = (await guild.GetUsersAsync());
            int usercount = users.Where(x => x.IsBot == false).Count();
            var gusers = await guild.GetUsersAsync();
            var gusersnobot = gusers.Where(x => x.IsBot == false);
            var gusersbotonly = gusers.Where(x => x.IsBot == true);
            var owner = await guild.GetOwnerAsync();
            string afkname = null;
            if(guild.AFKChannelId.HasValue)
                afkname = (await guild.GetVoiceChannelAsync(guild.AFKChannelId.Value)).Name;
            _embed.AddField("Users", usercount.ToString(),true);
            _embed.AddField("Bots", $"{gusersbotonly.Count()}",true);
            _embed.AddField("Shard", bot.GetShardIdFor(guild).ToString(),true);
            _embed.AddField("Verification Level", guild.VerificationLevel.ToString(),true);
            _embed.AddField("Voice Region ID", guild.VoiceRegionId.ToString(),true);
            _embed.AddField("Owner", owner.Nickname??owner.Username + "#" + owner.DiscriminatorValue,true);
            _embed.AddField("Text Channels", (await guild.GetTextChannelsAsync()).Count(),true);
            _embed.AddField("Voice Channels", (await guild.GetVoiceChannelsAsync()).Count(),true);
            int seconds = guild.AFKTimeout;
            string minutes = ((seconds % 3600) / 60).ToString();
            _embed.AddField("AFK Timeout", minutes + " minutes",true);
            _embed.AddField("AFK Channel",afkname??"None Set",true);
            if (String.IsNullOrEmpty(guild.IconUrl))
                _embed.AddField("Server Avatar", "Doesn't exist",true);
            _embed.AddField("Default Notifications", guild.DefaultMessageNotifications.ToString(),true);
            _embed.AddField("Created", guild.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            _embed.AddField("Emojis", guild.Emotes.Count.ToString() + Environment.NewLine + $"Use `{Bot.Prefix}server emojis` to view all of the emojis");
            _embed.AddField("Roles", guild.Roles.Count() + Environment.NewLine + $"Use `{Bot.Prefix}server roles` to view all of the roles");
            await MessageHandler.SendChannel((ITextChannel)Context.Channel,"", _embed.Build());
        }
        [Command("server emojis", RunMode = RunMode.Async), Alias("server emoji")]
        public async Task ServerEmoji()
        {
            var guild = Context.Guild;
            string message = null;
            var num = 0;
            message += $"Emojis of __**{guild.Name}**__" + Environment.NewLine;
            if (guild.Emotes.Count == 0)
                message += "Server contains no emotes";
            else
            {
                foreach (var emoji in guild.Emotes)
                {
                    if (emoji.Id == guild.Emotes.Last().Id)
                        message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}>";
                    else
                    {
                        if (num % 5 != 0)
                            message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}> | ";
                        else
                            message += $"{emoji.Name} <:{emoji.Name}:{emoji.Id}>\n";
                    }
                    num++;
                }
            }
            await MessageHandler.SendChannel(Context.Channel,message);
        }
        [Command("server roles", RunMode = RunMode.Async), Alias("server role")]
        public async Task ServerRoles()
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            string serverroles = null;
            foreach (var item in roles)
            {
                string thing = item.Name.TrimStart('@');
                if (item == guild.Roles.Last())
                    serverroles += thing;
                else
                    serverroles += thing + ", ";
            }
            string message = null;
            message += $"Roles of __**{guild.Name}**__" + Environment.NewLine;
            if (roles.Count == 0)
                message += "Server contains no roles";
            else
                message += "`" + serverroles + "`";
            await MessageHandler.SendChannel(Context.Channel,message);
        }

        [Command("bot", RunMode = RunMode.Async)]
        public async Task DInfo() =>
            await MessageHandler.SendDMs((ITextChannel)Context.Channel, await Context.User.GetOrCreateDMChannelAsync(), $"Hey. I'm Skuld, a utility bot that aims to bring tools and fun stuff to your server! :blush:\n\nThanks for using the bot. It means a lot. <3\n\nP.S. I also have a twitter where you can also get support from: <https://twitter.com/SkuldDiscordBot> \n\nOr there's the Support server: http://discord.gg/JYzvjah \n\n" +
                "P.S. from the Dev:\n" +
                "By using this command and all others, past-present-future, you consent to having your User Data be stored for proper functionality of the commands. This also applies to the server information if you are a server admin/moderator");

        [Command("id", RunMode = RunMode.Async), Summary("Gets the users ID only")]
        public async Task InstigatorgetID()
        {
            await MessageHandler.SendChannel(Context.Channel, $"Your ID is: `{Context.User.Id}`");
        }
        [Command("id guild", RunMode = RunMode.Async), Summary("Get ID of Guild")]
        public async Task GuildID()
        {
            await MessageHandler.SendChannel(Context.Channel, $"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`");
        }
        [Command("id", RunMode = RunMode.Async), Summary("Gets the users ID only")]
        public async Task GetID(IGuildUser user)
        {
            await MessageHandler.SendChannel(Context.Channel, $"The ID of **{user.Username + "#" + user.DiscriminatorValue}** is: `{user.Id}`");
        }
        [Command("id", RunMode = RunMode.Async), Summary("Get id of channel")]
        public async Task ChanID(IChannel channel)
        {
            await MessageHandler.SendChannel(Context.Channel, $"The ID of **{channel.Name}** is `{channel.Id}`");
        }

        [Command("serverinvite", RunMode = RunMode.Async), Summary("Gives discord invite")]
        public async Task DevDisc()
        {
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await MessageHandler.SendDMs((ITextChannel)Context.Channel, user, $"Join the support server at: http://discord.gg/JYzvjah");
        }
        [Command("botinvite", RunMode = RunMode.Async), Summary("OAuth2 Invite")]
        public async Task BotInvite()
        {
            var bot = await Skuld.Bot.bot.GetApplicationInfoAsync();
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await MessageHandler.SendDMs((ITextChannel)Context.Channel, user, $"Invite me using: https://discordapp.com/oauth2/authorize?client_id={bot.Id}&scope=bot&permissions=1073802246");
        }
        [Command("userratio", RunMode = RunMode.Async), Summary("Gets the ratio of users to bots")]
        public async Task HumanToBotRatio()
        {
            var guild = Context.Guild as SocketGuild;
            await guild.DownloadUsersAsync();
            int bots = guild.Users.Where(x => x.IsBot == true).Count();
            int humans = guild.Users.Where(x=>x.IsBot == false).Count();
            int guildusers = guild.Users.Count;
            var oldperc = (decimal)bots / guildusers*100m;
            var total = Math.Round(oldperc, 2);
            await MessageHandler.SendChannel(Context.Channel, $"Current Bots are: {bots}\nCurrent Users are: {humans}\nTotal Guild Users: {guildusers}\n{total}% of the Guild Users are bots");
        }
        [Command("avatar",RunMode = RunMode.Async), Summary("Gets your avatar url")]
        public async Task Avatar() =>
            await MessageHandler.SendChannel(Context.Channel, Context.User.GetAvatarUrl(ImageFormat.Auto) ?? "You have no avatar set");
        [Command("avatar", RunMode = RunMode.Async), Summary("Gets your avatar url")]
        public async Task Avatar([Remainder]IGuildUser user) =>
            await MessageHandler.SendChannel(Context.Channel, user.GetAvatarUrl(ImageFormat.Auto) ?? $"User {user.Nickname??user.Username} has no avatar set");

        [Command("mods", RunMode = RunMode.Async),Summary("Gives online status of Moderators/Admins")]
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
                        if (user.Game.HasValue &&user.Game.Value.StreamType != StreamType.NotStreaming)
                            adminstatus += streamingemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Online)
                            adminstatus += onlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle)
                            adminstatus += idleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.DoNotDisturb)
                            adminstatus += dontdisturbemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Invisible)
                            adminstatus += invisibleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Offline)
                            adminstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                    }
                    else if (user.GuildPermissions.KickMembers && user.GuildPermissions.ManageMessages)
                    {
                        if(user.Game.HasValue && user.Game.Value.StreamType != StreamType.NotStreaming)
                            modstatus += streamingemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Online)
                            modstatus += onlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle)
                            modstatus += idleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.DoNotDisturb)
                            modstatus += dontdisturbemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Invisible)
                            modstatus += invisibleemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                        if (user.Status == UserStatus.Offline)
                            modstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue + "\n";
                    }
                }
            }
            string message = "";
            if (modstatus != "__Moderators__\n")
                message = modstatus + "\n" + adminstatus;
            else
                message = adminstatus;
            await MessageHandler.SendChannel(Context.Channel, message);
        }
        [Command("createinvite",RunMode = RunMode.Async),Summary("Creates a new invite to the guild")]
        public async Task NewInvite(ITextChannel channel,int MaxAge,int MaxUses, bool Permanent, bool Unique)
        {
            IInviteMetadata invite;
            if (MaxAge>0 && MaxUses<0)
                invite = await channel.CreateInviteAsync(MaxAge, null, Permanent, Unique);
            else if (MaxAge<0 && MaxUses>0)
                invite = await channel.CreateInviteAsync(null, MaxUses, Permanent, Unique);
            else if (MaxAge<0 && MaxUses<0)
                invite = await channel.CreateInviteAsync(null, null, Permanent, Unique);
            else
                invite = await channel.CreateInviteAsync(MaxAge, MaxUses, Permanent, Unique);
            await MessageHandler.SendChannel(Context.Channel,
                "I created the invite with the following settings:\n" +
                "```cs\n" +
                $"       Channel : {channel.Name}\n"+
                $"   Maximum Age : {MaxAge}\n" +
                $"  Maximum Uses : {MaxUses}\n" +
                $"     Permanent : {Permanent}\n" +
                $"        Unique : {Unique}" +
                $"```Here's the link: {invite.Url}");

        }
        [Command("nick",RunMode =RunMode.Async),Summary("Gets the user behind the nickname")]
        public async Task Nickname([Remainder]IGuildUser user)
        {
            if (!String.IsNullOrEmpty(user.Nickname))
                await MessageHandler.SendChannel(Context.Channel, $"The user **{user.Username}#{user.DiscriminatorValue}** has a nickname of: {user.Nickname}");
            else
                await MessageHandler.SendChannel(Context.Channel, "This user doesn't have a nickname set.");
        }

        [Command("me", RunMode = RunMode.Async)]
        public async Task Whois() =>
    await GetProile(Context.User as IGuildUser);
        [Command("whois", RunMode = RunMode.Async), Summary("Get's information about a user"), Alias("user")]
        public async Task GetProile([Remainder]IGuildUser whois)
        {
            string Nickname = null;
            string status = "";
            if (!String.IsNullOrEmpty(whois.Nickname))
                Nickname = $"({whois.Nickname})";
            if (whois.Game.HasValue && whois.Game.Value.StreamType != StreamType.NotStreaming)
                status += streamingemote + " Streaming, ["+whois.Game.Value.Name+"]("+whois.Game.Value.StreamUrl+")";
            if (whois.Status == UserStatus.Online)
                status += onlineemote + " Online";
            if (whois.Status == UserStatus.AFK || whois.Status == UserStatus.Idle)
                status += idleemote + " AFK/Idle";
            if (whois.Status == UserStatus.DoNotDisturb)
                status += dontdisturbemote + " Do not Disturb/Busy";
            if (whois.Status == UserStatus.Invisible)
                status += invisibleemote + " Invisible";
            if (whois.Status == UserStatus.Offline)
                status += offlineemote + " Offline";

            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = RandColor.RandomColor(),
                Author = new EmbedAuthorBuilder() { IconUrl = whois.GetAvatarUrl(ImageFormat.Auto) ?? "", Name = whois.Username+$"#{whois.DiscriminatorValue} {Nickname??""}"  },
                ImageUrl = whois.GetAvatarUrl(ImageFormat.Auto) ?? ""
            };
            int seencount = 0;
            foreach (var item in Bot.bot.Guilds)
                if (item.GetUser(whois.Id) != null)
                    seencount++;
            string game = null;
            if (whois.Game.HasValue)
            {
                game = whois.Game.Value.Name;
            }
            else
                game = "Nothing";
            embed.AddField(":id: ID", whois.Id.ToString() ?? "Unknown",true);
            embed.AddField(":vertical_traffic_light:  Status", status ?? "Unknown",true);
            embed.AddField(":video_game: Currently Playing", game,true);
            embed.AddField(":robot: Bot?", whois.IsBot.ToString() ?? "Unknown",true);
            embed.AddField(":eyes: Mutual Servers", $"{seencount} servers",true);
            embed.AddField(":eyes: Last Seen", "Shard: " + (Bot.bot.GetShardIdFor(Context.Guild).ToString() ?? "Unknown"),true);
            embed.AddField(":shield: Roles", $"Do `{Config.Load().Prefix}roles` to see your roles");
            embed.AddField(":inbox_tray: Server Join", whois.JoinedAt.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            embed.AddField(":globe_with_meridians: Discord Join", whois.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            await MessageHandler.SendChannel(Context.Channel, "", embed.Build());
        }
        [Command("roles", RunMode = RunMode.Async), Summary("Gets your current roles")]
        public async Task GetRole() =>
            await GetRole(Context.User as IGuildUser);
        [Command("roles", RunMode = RunMode.Async), Summary("Gets a users current roles")]
        public async Task GetRole(IGuildUser user)
        {
            var guild = Context.Guild;
            var userroles = user.RoleIds;
            var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            string username = null;
            if (!String.IsNullOrEmpty(user.Nickname))
                username = user.Nickname + "#" + user.DiscriminatorValue;
            else
                username = user.Username + "#" + user.DiscriminatorValue;
            await MessageHandler.SendChannel(Context.Channel, "Roles of __**" + username + "**__\n\n`" + (roles ?? "No roles") + "`");
        }
        [Command("epoch", RunMode = RunMode.Async), Summary("Gets Current Time in Epoch")]
        public async Task Epoch()
        {
            var dtnowutc = DateTime.UtcNow;
            await MessageHandler.SendChannel(Context.Channel, $"The current time in UTC ({dtnowutc.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {(Int32)(dtnowutc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}");
        }
        [Command("epoch", RunMode = RunMode.Async), Summary("Gets a specific DateTime DD/MM/YYYY HH:MM:SS (24 Hour) in POSIX/Unix Epoch time")]
        public async Task Epoch([Remainder]string epoch)
        {
            DateTime datetime = Convert.ToDateTime(epoch, new CultureInfo("en-GB"));
            var epochdt = (Int32)(datetime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            await MessageHandler.SendChannel(Context.Channel, $"Your DateTime ({datetime.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}) in epoch is: {epochdt}");
        }
        [Command("epoch", RunMode = RunMode.Async), Summary("epoch to DateTime format")]
        public async Task Task(ulong epoch)
        {
            var epochtodt = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc).AddSeconds(Convert.ToDouble(epoch));
            await MessageHandler.SendChannel(Context.Channel, $"Your epoch ({epoch}) in DateTime is: {epochtodt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")}");
        }
    }
}
