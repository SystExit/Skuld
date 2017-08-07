using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Skuld.Tools;
using Discord.WebSocket;

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
            EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _auth.Name = guild.Name;
            if (!String.IsNullOrEmpty(guild.IconUrl))
                _embed.ThumbnailUrl = guild.IconUrl;
            _embed.Author = _auth;
            var users = (await guild.GetUsersAsync());
            int usercount = users.Where(x => x.IsBot == false).Count();
            var gusers = await guild.GetUsersAsync();
            var gusersnobot = gusers.Where(x => x.IsBot == false);
            var gusersbotonly = gusers.Where(x => x.IsBot == true);
            _embed.AddInlineField("Users", usercount.ToString());
            _embed.AddInlineField("Bots", $"{gusersbotonly.Count()}");
            _embed.AddInlineField("Shard", bot.GetShardIdFor(guild).ToString());
            _embed.AddInlineField("Verification Level", guild.VerificationLevel.ToString());
            _embed.AddInlineField("Voice Region ID", guild.VoiceRegionId.ToString());
            _embed.AddField(async x =>
            {
                var owner = await guild.GetOwnerAsync();
                x.IsInline = true;
                x.Name = "Owner";
                if (!String.IsNullOrEmpty(owner.Nickname))
                    x.Value = owner.Nickname + "#" + owner.DiscriminatorValue;
                else
                {
                    x.Value = owner.Username + "#" + owner.DiscriminatorValue;
                }
            });
            _embed.AddInlineField("Emojis", guild.Emotes.Count.ToString() + Environment.NewLine + $"Use `{Skuld.Bot.Prefix}server emojis` to view all of the emojis");
            int seconds = guild.AFKTimeout;
            string minutes = ((seconds % 3600) / 60).ToString();
            _embed.AddInlineField("AFK Timeout", minutes + " minutes");
            _embed.AddField(async x =>
            {
                x.IsInline = true;
                x.Name = "AFK Channel";
                if (guild.AFKChannelId.HasValue)
                    x.Value = (await guild.GetVoiceChannelAsync(guild.AFKChannelId.Value)).Name;
                else
                {
                    x.Value = "None Set";
                }
            });
            if (String.IsNullOrEmpty(guild.IconUrl))
            {
                _embed.AddInlineField("Server Avatar", "Doesn't exist");
            }
            _embed.AddInlineField("Created", guild.CreatedAt.ToString());
            _embed.AddInlineField("Roles", guild.Roles.Count() + Environment.NewLine + $"Use `{Skuld.Bot.Prefix}server roles` to view all of the roles");
            _embed.AddInlineField("Default Channel", (await guild.GetChannelAsync(guild.DefaultChannelId)).Name);
            _embed.AddInlineField("Default Notifications", guild.DefaultMessageNotifications.ToString());
            _embed.AddInlineField("Text Channels", (await guild.GetTextChannelsAsync()).Count());
            _embed.AddInlineField("Voice Channels", (await guild.GetVoiceChannelsAsync()).Count());
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
        [Command("aboutbot", RunMode = RunMode.Async)]
        public async Task DInfo()
        {
            var user = await Context.User.GetOrCreateDMChannelAsync();
            await MessageHandler.SendDMs(Context.Channel, user,$"Hey. I'm Skuld, a utility bot that aims to bring tools and fun stuff to your server! :blush:\n\nThanks for using the bot. It means a lot. <3");
        }

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
            await MessageHandler.SendDMs(Context.Channel, user, $"Join the support server at: http://discord.gg/6jcAP38");
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
            int guildusers = guild.DownloadedMemberCount;
            double perc = (double)bots / (double)guildusers;
            string percentage = Math.Round(perc, 2)*100+"%";
            await MessageHandler.SendChannel(Context.Channel, $"Current Bots are: {bots}\nCurrent Users are: {humans}\nTotal Guild Users: {guildusers}\n{percentage} of the Guild Users are bots");
        }
    }
}
