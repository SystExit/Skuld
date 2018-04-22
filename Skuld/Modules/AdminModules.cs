using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Discord;
using MySql.Data.MySqlClient;
using Discord.Addons.Interactive;
using StatsdClient;
using Discord.WebSocket;
using Skuld.Services;

namespace Skuld.Modules
{
    [Group,Name("Admin"),RequireRole(AccessLevel.ServerMod)]
    public class Admin : InteractiveBase<ShardedCommandContext>
    {
		readonly DatabaseService database;
		readonly LoggingService logger;
		readonly MessageService messageService;

		public Admin(DatabaseService db,
			LoggingService log,
			MessageService message)
		{
			database = db;
			logger = log;
			messageService = message;
		}

        [Command("say", RunMode = RunMode.Async), Summary("Say something to a channel")]
        public async Task Say(IMessageChannel channel, [Remainder]string message) =>
            await messageService.SendChannelAsync(channel, message);

        [Command("roleids", RunMode = RunMode.Async), Summary("Gets all role ids")]
        public async Task GetRoleIds()
        {
            string lines = "";
            var guild = Context.Guild;
            var roles = guild.Roles;

            foreach (var item in roles)            
                lines = lines + $"{Convert.ToString(item.Id)} - \"{item.Name}\""+Environment.NewLine;
            
            if (lines.Length > 2000)
            {
                var paddedlines = lines.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var pagesold = new List<string>();
                int prev = 0;
                for (int x = 1; x <= lines.Length; x++)
                {
                    if (lines.Length % x == 0 && x > 9)
                    {
                        pagesold.Add("```cs\n" + string.Join(",\n", paddedlines.Skip(prev).Take(x)) + "```");
                        prev = x;
                    }
                    if (x == lines.Length)                    
                        pagesold.Add("```cs\n" + string.Join(",\n", paddedlines.Skip(prev).Take(x)) + "```");                    
                }
                var pages = new List<string>();
                foreach (var page in pagesold)
                {
                    if (page != "```cs\n```")                    
                        pages.Add(page);                    
                }
                await PagedReplyAsync(pages, fromSourceUser: true);
            }
            else
				await messageService.SendChannelAsync(Context.Channel, "```cs\n" + lines + "```");
        }

        [Command("mute", RunMode = RunMode.Async), Summary("Mutes a user")]
		[RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = guild.TextChannels;
            var cmd = new MySqlCommand("SELECT MutedRole from guild WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var roleid = await database.GetSingleAsync(cmd);
            if (String.IsNullOrEmpty(roleid))
            {
                var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None);
                foreach (var chan in channels)
                {
                    await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan));
                }
                cmd = new MySqlCommand("UPDATE guild SET MutedRole = @roleid WHERE ID = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                cmd.Parameters.AddWithValue("@roleid", role.Id);
                await database.NonQueryAsync(cmd);
                await user.AddRoleAsync(role);
                await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
            }
            else
            {
                var role = guild.GetRole(Convert.ToUInt64(roleid));
                await user.AddRoleAsync(role);
                await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
            }
        }

        [Command("unmute", RunMode = RunMode.Async), Summary("Unmutes a user")]
		[RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(IUser usertounmute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertounmute as IGuildUser;
            var channels = guild.TextChannels;
            var cmd = new MySqlCommand("SELECT MutedRole from guild WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var roleid = await database.GetSingleAsync(cmd);
            if (String.IsNullOrEmpty(roleid))
            {
                await messageService.SendChannelAsync(Context.Channel, "Role doesn't exist, so I cannot unmute");
                StatsdClient.DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
            }
            else
            {
                var role = guild.GetRole(Convert.ToUInt64(roleid));
                await user.RemoveRoleAsync(role);
                await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just unmuted **{usertounmute.Username}**");
            }
        }

        [Command("prune", RunMode = RunMode.Async), Summary("Cleans set amount of messages.")]
		[RequireBotAndUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(int amount, IUser user = null)
		{
			if (amount < 0)
			{
				await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} Your amount `{amount}` is under 0.");
				StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
				return;
			}
			await Context.Message.DeleteAsync();
			if (user==null)
			{
				var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
				ITextChannel chan = (ITextChannel)Context.Channel;
				await chan.DeleteMessagesAsync(messages).ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{ await messageService.SendChannelAsync(Context.Channel, ":ok_hand: Done!", 5); }
				});				
			}
			else
			{
				var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
				var usermessages = messages.Where(x => x.Author.Id == user.Id);
				usermessages = usermessages.Take(amount);
				ITextChannel chan = (ITextChannel)Context.Channel;
				await chan.DeleteMessagesAsync(usermessages).ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{ await messageService.SendChannelAsync(Context.Channel, ":ok_hand: Done!", 5); }
				});
			}
        }

		[Command("kick", RunMode = RunMode.Async), Summary("Kicks a user"), Alias("dab", "dabon")]
		[RequireBotAndUserPermission(GuildPermission.KickMembers)]
		public async Task Kick(IGuildUser user, [Remainder]string reason = null)
		{
			var msg = $"You have been kicked from **{Context.Guild.Name}** by: {Context.User.Username}#{Context.User.Discriminator}";
			if (reason == null)
			{
				try
				{
					var dmchan = await user.GetOrCreateDMChannelAsync();
					await dmchan.SendMessageAsync(msg);
				}
				catch
				{ /*Can be Ignored lol*/ }
				await user.KickAsync($"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{
						await messageService.SendChannelAsync(Context.Channel, $"Successfully kicked: `{user.Username}`\tResponsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
					}
				});
			}
			else
			{
				msg += $" with reason:```\n{reason}```";
				try
				{
					var dmchan = await user.GetOrCreateDMChannelAsync();
					await dmchan.SendMessageAsync(msg);
				}
				catch
				{ /*Can be Ignored lol*/ }
				await user.KickAsync(reason+$" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{
						await messageService.SendChannelAsync(Context.Channel, $"Successfully kicked: `{user}`\tResponsible Moderator: {Context.User}\nReason: " + reason);
					}
				});
			}
		}

		[Command("ban", RunMode = RunMode.Async), Summary("Bans a user"), Alias("naenae")]
		[RequireBotAndUserPermission(GuildPermission.BanMembers)]
		public async Task Ban(IGuildUser user, int daystoprune = 7, [Remainder]string reason = null)
		{
			var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
			if (reason == null)
			{
				try
				{
					var dmchan = await user.GetOrCreateDMChannelAsync();
					await dmchan.SendMessageAsync(msg);
				}
				catch { }
				await Context.Guild.AddBanAsync(user, daystoprune, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
				await messageService.SendChannelAsync(Context.Channel, $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
			}
			else
			{
				msg += $" with reason:```\n{reason}```";
				try
				{
					var dmchan = await user.GetOrCreateDMChannelAsync();
					await dmchan.SendMessageAsync(msg);
				}
				catch { }
				await Context.Guild.AddBanAsync(user, daystoprune, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
				await messageService.SendChannelAsync(Context.Channel, $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator:{Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}");
			}
		}

        [Command("hackban", RunMode = RunMode.Async), Summary("Hackbans a set of userids Must be in this format hackban [id1],[id2],[id3]")]
		[RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task HackBan(params string[] ids)
        {
			if (ids.Count() > 0)
			{
				foreach (var id in ids)
					await Context.Guild.AddBanAsync(Convert.ToUInt64(id));

				await messageService.SendChannelAsync(Context.Channel, $"Banned IDs: {ids}");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, $"Couldn't parse list of ID's.");
				DogStatsd.Increment("commands.errors",1,1,new string[] { "parse-fail" });
			}
		}
		
		[Command("softban", RunMode = RunMode.Async), Summary("Softbans a user")]
		[RequireBotAndUserPermission(GuildPermission.BanMembers)]
		public async Task SoftBan(IUser user, [Remainder]string reason = null)
		{
			var newreason = $"Softban - Responsible Moderator: {Context.User.Username}#{Context.User.DiscriminatorValue}";
			if(reason == null)
			{
				await Context.Guild.AddBanAsync(user, 7, newreason);
				await messageService.SendChannelAsync(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`");
				await Context.Guild.RemoveBanAsync(user);
			}
			else
			{
				newreason += " - Reason: " + reason;
				await Context.Guild.AddBanAsync(user, 7, newreason);
				await messageService.SendChannelAsync(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}");
				await Context.Guild.RemoveBanAsync(user);
			}
		}

		[Command("setjrole", RunMode = RunMode.Async), Summary("Allows a role to be auto assigned on userjoin"), RequireDatabase]
		public async Task AutoRole(IRole role = null)
		{
			if (role == null)
			{
				var guild = Context.Guild;
				var cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
				cmd.Parameters.AddWithValue("@guildid", guild.Id);
				var reader = await database.GetSingleAsync(cmd);
				if (reader != null)
				{
					cmd = new MySqlCommand("UPDATE guild SET autojoinrole = NULL WHERE ID = @guildid");
					cmd.Parameters.AddWithValue("@guildid", guild.Id);
					await database.NonQueryAsync(cmd).ContinueWith(async x =>
					{
						await messageService.SendChannelAsync(Context.Channel, $"Successfully removed the member join role");
					});
				}
			}
			else
			{
				var guild = Context.Guild;
				var cmd = new MySqlCommand("UPDATE guild SET autojoinrole = @roleid WHERE ID = @guildid");
				cmd.Parameters.AddWithValue("@guildid", guild.Id);
				cmd.Parameters.AddWithValue("@roleid", role.Id);
				await database.NonQueryAsync(cmd).ContinueWith(async x =>
				{
					cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
					cmd.Parameters.AddWithValue("@guildid", guild.Id);
					var reader = await database.GetSingleAsync(cmd);
					if (reader != null)
					{ await messageService.SendChannelAsync(Context.Channel, $"Successfully set **{role.Name}** as the member join role"); }
				});
			}
        }

        [Command("autorole", RunMode = RunMode.Async), Summary("Get's guilds current autorole"), RequireDatabase]
        public async Task AutoRole()
        {
            var guild = Context.Guild;
            var cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var reader = await database.GetSingleAsync(cmd);
            if (!string.IsNullOrEmpty(reader))
            { await messageService.SendChannelAsync(Context.Channel, $"**{guild.Name}**'s current auto role is `{guild.GetRole(Convert.ToUInt64(reader)).Name}`"); }
            else
            { await messageService.SendChannelAsync(Context.Channel, $"Currently, **{guild.Name}** has no auto role."); }
        }

        [Command("setprefix", RunMode = RunMode.Async), Summary("Sets the prefix, or resets on empty prefix"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix = null)
        {
			if(prefix!=null)
			{
				var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
				cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
				string oldprefix = await database.GetSingleAsync(cmd);

				cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
				cmd.Parameters.AddWithValue("@prefix", prefix);
				cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
				await database.NonQueryAsync(cmd).ContinueWith(async x =>
				{
					cmd = new MySqlCommand("select prefix from guild where id = @guildid");
					cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
					var result = await database.GetSingleAsync(cmd);
					if (result != oldprefix)
					{ await messageService.SendChannelAsync(Context.Channel, $"Successfully set `{prefix}` as the Guild's prefix"); }
					else
					{ await messageService.SendChannelAsync(Context.Channel, $":thinking: It didn't change. Probably because it is the same as the current prefix."); }
				});
			}
			else
			{
				var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
				cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
				string oldprefix = await database.GetSingleAsync(cmd);

				cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
				cmd.Parameters.AddWithValue("@prefix", Bot.Configuration.Prefix);
				cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);

				await database.NonQueryAsync(cmd).ContinueWith(async x =>
				{
					cmd = new MySqlCommand("select prefix from guild where id = @guildid");
					cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
					var result = await database.GetSingleAsync(cmd);
					if (result != oldprefix)
					{ await messageService.SendChannelAsync(Context.Channel, $"Successfully reset the Guild's prefix"); }
				});
			}
        }

        //Set Channel
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome([Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await database.NonQueryAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Set Welcome message!");
            });
        }

        //Current Channel
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await database.NonQueryAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Set Welcome message!");
            });
        }

        //Deletes
        [Command("unsetwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome()
        {
            var cmd = new MySqlCommand("UPDATE guild SET joinmessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Removed Welcome message!");
            });            
        }

        //Set Channel
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await database.NonQueryAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Set Leave message!");
            });
        }

        //Current Channel
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave([Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await database.NonQueryAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Set Leave message!");
            });
        }

        //Deletes
        [Command("unsetleave", RunMode = RunMode.Async), Summary("Clears the leave message"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave()
        {
            var cmd = new MySqlCommand("UPDATE guild SET leavemessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await database.NonQueryAsync(cmd).ContinueWith(async x =>
            {
                await messageService.SendChannelAsync(Context.Channel, $"Removed Leave message!");
            });
        }

		[Command("addcommand", RunMode = RunMode.Async), Summary("Adds a custom command"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task AddCustomCommand(string name, [Remainder]string content)
		{
			if(name.Contains('.')||name.Contains("www.")||name.Contains("http://")||name.Contains("https://"))
			{
				await messageService.SendChannelAsync(Context.Channel, "Commands can't be a url/website", 5);
				return;
			}
			if(name.Split(' ').Length>1)
			{
				await messageService.SendChannelAsync(Context.Channel, "Commands can't contain a space", 5);
				return;
			}
			else
			{
				var cmdsearch = messageService.commandService.Search(Context, name);
				if (cmdsearch.Commands!=null)
				{
					await messageService.SendChannelAsync(Context.Channel, "The bot already has this command", 5);
				}
				else
				{
					var custcmd = await database.GetCustomCommandAsync(Context.Guild.Id, name);
					if (custcmd != null)
					{
						await messageService.SendChannelAsync(Context.Channel, $"Custom command named `{custcmd.CommandName}` already exists, overwrite with new content? Y/N", 5);
						var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5));
						if(msg!=null)
						{
							if (msg.Content.ToLower() == "y")
							{
								var cmd = new MySqlCommand("UPDATE `customcommand` SET `Content` = @newcontent WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
								cmd.Parameters.AddWithValue("@newcontent", content);
								cmd.Parameters.AddWithValue("@guildID", Context.Guild.Id);
								cmd.Parameters.AddWithValue("@commandName", name);
								await database.NonQueryAsync(cmd);
								await messageService.SendChannelAsync(Context.Channel, $"Updated the command.");
							}
						}
						else
							await messageService.SendChannelAsync(Context.Channel, "Reply timed out, not updating.", 5);
						return;
					}
					else
					{
						var cmd = new MySqlCommand("INSERT INTO `customcommand` ( `Content`, `GuildID`, `CommandName` ) VALUES ( @newcontent , @guildID , @commandName ) ;");
						cmd.Parameters.AddWithValue("@newcontent", content);
						cmd.Parameters.AddWithValue("@guildID", Context.Guild.Id);
						cmd.Parameters.AddWithValue("@commandName", name);
						await database.NonQueryAsync(cmd);
						await messageService.SendChannelAsync(Context.Channel, $"Added the command.");
					}
				}				
			}
		}

		[Command("deletecommand", RunMode = RunMode.Async), Summary("Deletes a custom command"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task DeleteCustomCommand(string name)
		{
			if (name.Split(' ').Length > 1)
			{
				await messageService.SendChannelAsync(Context.Channel, "Commands can't contain a space");
				return;
			}
			else
			{				
				var cmd = new MySqlCommand("DELETE FROM `customcommand` WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
				cmd.Parameters.AddWithValue("@guildID", Context.Guild.Id);
				cmd.Parameters.AddWithValue("@commandName", name);
				await database.NonQueryAsync(cmd);
				await messageService.SendChannelAsync(Context.Channel, $"Deleted the command.");							
			}
		}

        [Command("guildfeature", RunMode = RunMode.Async), Summary("Configures guild features"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildFeatures(string module, int value)
        {
            if (value > 1)
            { await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder() { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build()); }
            if (value < 0)
            { await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder() { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build()); }
            else
            {
                module = module.ToLowerInvariant();
                var settings = new Dictionary<string, string>()
                {
                    {"pinning","pinning" },
                    {"levels","experience" }
                };
                if (settings.ContainsKey(module) || settings.ContainsValue(module))
                {
                    if (!String.IsNullOrEmpty(await database.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildfeaturemodules` WHERE `ID` = " + Context.Guild.Id))))
                    {
                        var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);
                        await database.NonQueryAsync(new MySqlCommand($"UPDATE `guildfeaturemodules` SET `{setting.Value}` = {value} WHERE `ID` = {Context.Guild.Id}"));
                        if (value == 0)
                        { await messageService.SendChannelAsync(Context.Channel, $"I disabled the `{module}` feature"); }
                        else
                        { await messageService.SendChannelAsync(Context.Channel, $"I enabled the `{module}` feature"); }
                    }
                    else
                    {
                        await database.InsertAdvancedSettingsAsync(feature: false, guild: Context.Guild as Discord.WebSocket.SocketGuild);
                    }
                }
                else
                {
                    string modulelist = "";
                    foreach (var mod in settings)
                    { modulelist += mod.Key + " (" + mod.Value + ")" + ", "; }
                    modulelist = modulelist.Remove(modulelist.Length - 2);
                    await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
                }
            }
        }

        [Command("guildmodule", RunMode = RunMode.Async), Summary("Configures guild modules"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildModules(string module, int value)
        {
            if (value > 1)
            { await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build()); }
            if (value < 0)
            { await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build()); }
            else
            {
                module = module.ToLowerInvariant();
                string[] modules = { "accounts", "actions", "admin", "fun", "help", "information", "search", "stats", "ai" };
                if (modules.Contains(module))
                {
                    if (!String.IsNullOrEmpty(await database.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildcommandmodules` WHERE `ID` = " + Context.Guild.Id))))
                    {
                        await database.NonQueryAsync(new MySqlCommand($"UPDATE `guildcommandmodules` SET `{module}` = {value} WHERE `ID` = {Context.Guild.Id}"));
                        if (value == 0)
                        { await messageService.SendChannelAsync(Context.Channel, $"I disabled the `{module}` module"); }
                        else
                        { await messageService.SendChannelAsync(Context.Channel, $"I enabled the `{module}` module"); }
                    }
                    else
                    {
                        await database.InsertAdvancedSettingsAsync(feature: false, guild: Context.Guild as Discord.WebSocket.SocketGuild);
                    }
                }
                else
                {
                    string modulelist = string.Join(", ", modules);
                    modulelist = modulelist.Remove(modulelist.Length - 2);
                    await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
                }
            }
        }

        [Command("configurechannel", RunMode = RunMode.Async), Summary("Some features require channels to be set"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureChannel(string module, IChannel channel)
        {
            module = module.ToLowerInvariant();
            var modules = new Dictionary<string, string>()
            {
                {"twitchnotif","twitchnotifchannel" },
                {"twitch","twitchnotifchannel" },
                {"twitchnotification","twitchnotifchannel" },
                {"twitchnotifications","twitchnotifchannel" },
                {"twitter","twitterlogchannel" },
                {"twitterlog","twitterlogchannel" },
                {"userjoin","userjoinchan" },
                {"userjoined","userjoinchan" },
                {"userleave","userleavechan" },
                {"userleft","userleavechan" }
			};
            if (modules.ContainsKey(module)||modules.ContainsValue(module))
            {
                if (!String.IsNullOrEmpty(await database.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildcommandmodules` WHERE `ID` = " + Context.Guild.Id))))
                {
                    var setting = modules.FirstOrDefault(x => x.Key == module || x.Value == module);
                    await database.NonQueryAsync(new MySqlCommand($"UPDATE `guild` SET `{setting.Value}` = {channel.Id} WHERE `ID` = {Context.Guild.Id}"));
                    await messageService.SendChannelAsync(Context.Channel, $"I set `{channel.Name}` as the channel for the `{module}` module");
                }
                else
                {
                    await database.InsertAdvancedSettingsAsync(feature: false, guild: Context.Guild as Discord.WebSocket.SocketGuild);
                }
            }
            else
            {
                string modulelist = string.Join(", ", modules);
                modulelist = modulelist.Remove(modulelist.Length - 2);
                await messageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
            }
        }
    }
}
