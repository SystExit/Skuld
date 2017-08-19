using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Skuld.Tools;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Skuld.Commands
{
    [Group,Name("Information")]
    public class Information : ModuleBase
    {
        [Command("server", RunMode = RunMode.Async), Summary("Gets information about the server")]
        public async Task GetServer()
        {
            var bot = Skuld.Bot.bot;
            var guild = Context.Guild;
            var roles = guild.Roles;
            EmbedBuilder _embed = new EmbedBuilder();
            _embed.Color = RandColor.RandomColor();
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
            _embed.AddInlineField("Users", usercount.ToString());
            _embed.AddInlineField("Bots", $"{gusersbotonly.Count()}");
            _embed.AddInlineField("Shard", bot.GetShardIdFor(guild).ToString());
            _embed.AddInlineField("Verification Level", guild.VerificationLevel.ToString());
            _embed.AddInlineField("Voice Region ID", guild.VoiceRegionId.ToString());
            _embed.AddInlineField("Owner", owner.Nickname??owner.Username + "#" + owner.DiscriminatorValue);
            _embed.AddInlineField("Text Channels", (await guild.GetTextChannelsAsync()).Count());
            _embed.AddInlineField("Voice Channels", (await guild.GetVoiceChannelsAsync()).Count());
            int seconds = guild.AFKTimeout;
            string minutes = ((seconds % 3600) / 60).ToString();
            _embed.AddInlineField("AFK Timeout", minutes + " minutes");
            _embed.AddInlineField("AFK Channel",afkname??"None Set");
            if (String.IsNullOrEmpty(guild.IconUrl))
            {
                _embed.AddInlineField("Server Avatar", "Doesn't exist");
            }
            _embed.AddInlineField("Default Notifications", guild.DefaultMessageNotifications.ToString());            
            _embed.AddInlineField("Created", guild.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            _embed.AddInlineField("Emojis", guild.Emotes.Count.ToString() + Environment.NewLine + $"Use `{Skuld.Bot.Prefix}server emojis` to view all of the emojis");
            _embed.AddInlineField("Roles", guild.Roles.Count() + Environment.NewLine + $"Use `{Skuld.Bot.Prefix}server roles` to view all of the roles");
            await MessageHandler.SendChannel(Context.Channel,"", _embed);
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
            await MessageHandler.SendDMs(Context.Channel, await Context.User.GetOrCreateDMChannelAsync(), $"Hey. I'm Skuld, a utility bot that aims to bring tools and fun stuff to your server! :blush:\n\nThanks for using the bot. It means a lot. <3\n\nP.S. I also have a twitter where you can also get support from: <https://twitter.com/SkuldDiscordBot> \n\nOr there's the Support server: http://discord.gg/JYzvjah \n\n" +
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
            await MessageHandler.SendDMs(Context.Channel, user, $"Join the support server at: http://discord.gg/JYzvjah");
        }
        [Command("botinvite", RunMode = RunMode.Async), Summary("OAuth2 Invite")]
        public async Task Bot()
        {
            var bot = await Skuld.Bot.bot.GetApplicationInfoAsync();
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await MessageHandler.SendDMs(Context.Channel, user, $"Invite me using: https://discordapp.com/oauth2/authorize?client_id={bot.Id}&scope=bot&permissions=1073802246");
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

        [Command("mods", RunMode = RunMode.Async),Summary("Gives online status of Moderators/Admins")]
        public async Task ModsOnline()
        {
            await Context.Guild.DownloadUsersAsync();
            string streamingemote = "<:streaming:313956277132853248>";
            string onlineemote = "<:online:313956277808005120>";
            string idleemote = "<:away:313956277220802560>";
            string dontdisturbemote = "<:dnd:313956276893646850>";
            string invisibleemote = "<:invisible:313956277107556352>";
            string offlineemote = "<:offline:313956277237710868>";
            List<IGuildUser> Mods = new List<IGuildUser>();
            List<IGuildUser> Admins = new List<IGuildUser>();
            foreach (var user in await Context.Guild.GetUsersAsync())
            {
                if (!user.IsBot)
                {
                    if (user.GuildPermissions.Administrator)
                        Admins.Add(user);
                    else if (user.GuildPermissions.KickMembers && user.GuildPermissions.ManageMessages)
                        Mods.Add(user);
                }
            }
            string modstatus = "__Moderators__\n";
            string adminstatus = "__Administrators__\n";
            Console.WriteLine(Mods.Count);
            Console.WriteLine(modstatus);
            Console.WriteLine(adminstatus);
            int test = 0;
            foreach(var user in Mods)
            {
                Console.WriteLine(user.Username);
                Console.WriteLine("mods " + test++);
                if (user.Game.Value.StreamType != StreamType.NotStreaming)
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
                    modstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                Console.WriteLine(modstatus);
            }
            test = 0;
            foreach (var user in Admins)
            {
                Console.WriteLine("admin "+test++);
                if (user.Game.Value.StreamType != StreamType.NotStreaming)
                    adminstatus += streamingemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                if (user.Status == UserStatus.Online)
                    adminstatus += onlineemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle)
                    adminstatus += idleemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                if (user.Status == UserStatus.DoNotDisturb)
                    adminstatus += dontdisturbemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                if (user.Status == UserStatus.Invisible)
                    adminstatus += invisibleemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
                if (user.Status == UserStatus.Offline)
                    adminstatus += offlineemote + " " + user.Username + "#" + user.DiscriminatorValue+"\n";
            }

            Console.WriteLine(modstatus);
            Console.WriteLine(adminstatus);
            string message = modstatus + "\n\n" + adminstatus;
            await MessageHandler.SendChannel(Context.Channel, message);
        }
    }
}
