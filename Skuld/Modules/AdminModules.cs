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

        [Command("say"), Summary("Say something to a channel")]
        public async Task Say(IMessageChannel channel, [Remainder]string message) =>
            await messageService.SendChannelAsync(channel, message);

        [Command("roleids"), Summary("Gets all role ids")]
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

        [Command("mute"), Summary("Mutes a user")]
		[RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = guild.TextChannels;
			var sguild = await database.GetGuildAsync(guild.Id);

            if (sguild.MutedRole == 0)
            {
                var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None);
                foreach (var chan in channels)
                {
                    await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan));
                }
				sguild.MutedRole = role.Id;
				var resp = await database.UpdateGuildAsync(sguild);
				if(resp.Successful)
				{
					await user.AddRoleAsync(role);
					await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
					await logger.AddToLogsAsync(new Models.LogMessage("MuteCMD", resp.Error, LogSeverity.Error, resp.Exception));
				}
            }
            else
            {
                var role = guild.GetRole(sguild.MutedRole);
                await user.AddRoleAsync(role);
                await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
            }
        }

        [Command("unmute"), Summary("Unmutes a user")]
		[RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(IUser usertounmute)
        {
			var guild = Context.Guild;
			var roles = guild.Roles;
			var user = usertounmute as IGuildUser;
			var channels = guild.TextChannels;
			var sguild = await database.GetGuildAsync(guild.Id);

			if (sguild.MutedRole == 0)
			{
				await messageService.SendChannelAsync(Context.Channel, "Role doesn't exist, so I cannot unmute");
				DogStatsd.Increment("commands.errors",1,1, new string[]{ "generic" });
			}
			else
			{
				var role = guild.GetRole(sguild.MutedRole);
				await user.RemoveRoleAsync(role);
				await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} just unmuted **{usertounmute.Username}**");
			}
		}

        [Command("prune"), Summary("Cleans set amount of messages.")]
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

		[Command("kick"), Summary("Kicks a user"), Alias("dab", "dabon")]
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

		[Command("ban"), Summary("Bans a user"), Alias("naenae")]
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

        [Command("hackban"), Summary("Hackbans a set of userids Must be in this format hackban [id1],[id2],[id3]")]
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
		
		[Command("softban"), Summary("Softbans a user")]
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

		[Command("setjrole"), Summary("Allows a role to be auto assigned on userjoin"), RequireDatabase]
		public async Task AutoRole(IRole role = null)
		{
			var guild = Context.Guild;
			var sguild = await database.GetGuildAsync(guild.Id);
			if (role == null)
			{
				if(sguild.AutoJoinRole != 0)
				{
					sguild.AutoJoinRole = 0;
					var resp = await database.UpdateGuildAsync(sguild);
					if (resp.Successful)
					{
						await messageService.SendChannelAsync(Context.Channel, $"Successfully removed the member join role");
					}
					else
					{

						await logger.AddToLogsAsync(new Models.LogMessage("AJClCmd", resp.Error, LogSeverity.Error, resp.Exception));
					}
				}
			}
			else
			{
				sguild.AutoJoinRole = role.Id;
				var resp = await database.UpdateGuildAsync(sguild);
				if (resp.Successful)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Successfully set **{role.Name}** as the member join role");
				}
				else
				{

					await logger.AddToLogsAsync(new Models.LogMessage("AJClCmd", resp.Error, LogSeverity.Error, resp.Exception));
				}
			}
        }

        [Command("autorole"), Summary("Get's guilds current autorole"), RequireDatabase]
        public async Task AutoRole()
        {
			var sguild = await database.GetGuildAsync(Context.Guild.Id);

			if(sguild.AutoJoinRole == 0)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Currently, **{Context.Guild.Name}** has no auto role.");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, $"**{Context.Guild.Name}**'s current auto role is `{Context.Guild.GetRole(Convert.ToUInt64(sguild.AutoJoinRole)).Name}`");
			}
        }

        [Command("setprefix"), Summary("Sets the prefix, or resets on empty prefix"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix = null)
        {
			var sguild = await database.GetGuildAsync(Context.Guild.Id);

			if(prefix!=null)
			{
				var oldprefix = sguild.Prefix;
				sguild.Prefix = prefix;
				var resp = await database.UpdateGuildAsync(sguild);
				sguild = await database.GetGuildAsync(sguild.ID);

				if(resp.Successful)
				{
					if (sguild.Prefix != oldprefix)
					{
						await messageService.SendChannelAsync(Context.Channel, $"Successfully set `{prefix}` as the Guild's prefix");
					}
					else
					{
						await messageService.SendChannelAsync(Context.Channel, $":thinking: It didn't change. Probably because it is the same as the current prefix.");
					}
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
				}
			}
			else
			{
				sguild.Prefix = Bot.Configuration.Discord.Prefix;

				var resp = await database.UpdateGuildAsync(sguild);

				if(resp.Successful)
				{
					await messageService.SendChannelAsync(Context.Channel, $"Successfully reset the Guild's prefix");
				}
				else
				{
					await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
				}
			}
        }

        //Set Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome([Remainder]string welcome)
        {
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserJoinChannel = Context.Channel.Id;
			sguild.JoinMessage = welcome;
			var resp = await database.UpdateGuildAsync(sguild);
			if(resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Set Welcome message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
        }

        //Current Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserJoinChannel = channel.Id;
			sguild.JoinMessage = welcome;
			var resp = await database.UpdateGuildAsync(sguild);
			if (resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Set Welcome message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
		}

        //Deletes
        [Command("unsetwelcome"), Summary("Clears the welcome message"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome()
        {
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserJoinChannel = 0;
			sguild.JoinMessage = "";
			var resp = await database.UpdateGuildAsync(sguild);
			if (resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Cleared Welcome message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
		}

        //Set Channel
        [Command("setleave"), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
		{
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserLeaveChannel = channel.Id;
			sguild.LeaveMessage = leave;
			var resp = await database.UpdateGuildAsync(sguild);
			if (resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Set Leave message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
		}

        //Current Channel
        [Command("setleave"), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave([Remainder]string leave)
		{
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserLeaveChannel = Context.Channel.Id;
			sguild.LeaveMessage = leave;
			var resp = await database.UpdateGuildAsync(sguild);
			if (resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Set Leave message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
		}

        //Deletes
        [Command("unsetleave"), Summary("Clears the leave message"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave()
		{
			var sguild = await database.GetGuildAsync(Context.Guild.Id);
			sguild.UserLeaveChannel = 0;
			sguild.LeaveMessage = "";
			var resp = await database.UpdateGuildAsync(sguild);
			if (resp.Successful)
			{
				await messageService.SendChannelAsync(Context.Channel, $"Set Leave message!");
			}
			else
			{
				await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
			}
		}

		[Command("addcommand"), Summary("Adds a custom command"), RequireDatabase]
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
						if (msg != null)
						{
							if (msg.Content.ToLower() == "y")
							{
								var resp = await database.UpdateCustomCommand(Context.Guild, name, content);

								if (resp.Successful)
								{
									await messageService.SendChannelAsync(Context.Channel, $"Updated the command.");
								}
								else
								{
									await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
								}
							}
						}
						else
						{
							await messageService.SendChannelAsync(Context.Channel, "Reply timed out, not updating.", 5);
						}
						return;
					}
					else
					{
						var resp = await database.InsertCustomCommand(Context.Guild, name, content);

						if (resp.Successful)
						{
							await messageService.SendChannelAsync(Context.Channel, $"Added the command.");
						}
						else
						{
							await messageService.SendChannelAsync(Context.Channel, "Something happened. <:blobsick:350673776071147521>");
						}
					}
				}				
			}
		}

		[Command("deletecommand"), Summary("Deletes a custom command"), RequireDatabase]
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
				await messageService.SendChannelAsync(Context.Channel, $"Are you sure? Y/N", 5);
				var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5));
				if (msg != null)
				{
					if (msg.Content.ToLower() == "y")
					{
						await database.DropCustomCommand(Context.Guild, name);
						await messageService.SendChannelAsync(Context.Channel, $"Deleted the command.");
					}
				}							
			}
		}

        [Command("guildfeature"), Summary("Configures guild features"), RequireDatabase]
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
                    {"pinning","pinning" }//,
                    //{"levels","experience" }
                };
                if (settings.ContainsKey(module) || settings.ContainsValue(module))
                {
					var guild = await database.GetGuildAsync(Context.Guild.Id);

                    if (guild.GuildSettings.Modules!=null)
                    {
                        var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);
						
						switch(setting.Value)
						{
							case "pinning":
								guild.GuildSettings.Features.Pinning = Convert.ToBoolean(value);
								break;

							case "experience":
								guild.GuildSettings.Features.Experience = Convert.ToBoolean(value);
								break;
						}

                        await database.UpdateGuildAsync(guild);
                        if (value == 0)
                        {
							await messageService.SendChannelAsync(Context.Channel, $"I disabled the `{module}` feature");
						}
                        else
                        {
							await messageService.SendChannelAsync(Context.Channel, $"I enabled the `{module}` feature");
						}
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

        [Command("guildmodule"), Summary("Configures guild modules"), RequireDatabase]
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
                string[] modules = { "accounts", "actions", "admin", "fun", "help", "information", "search", "stats" };
                if (modules.Contains(module))
                {
					var guild = await database.GetGuildAsync(Context.Guild.Id);
                    if (guild.GuildSettings.Modules!=null)
                    {
						switch(module)
						{
							case "accounts":
								guild.GuildSettings.Modules.AccountsEnabled = Convert.ToBoolean(value);
								break;

							case "actions":
								guild.GuildSettings.Modules.ActionsEnabled = Convert.ToBoolean(value);
								break;

							case "admin":
								guild.GuildSettings.Modules.AdminEnabled = Convert.ToBoolean(value);
								break;

							case "fun":
								guild.GuildSettings.Modules.FunEnabled = Convert.ToBoolean(value);
								break;

							case "help":
								guild.GuildSettings.Modules.HelpEnabled = Convert.ToBoolean(value);
								break;

							case "information":
								guild.GuildSettings.Modules.InformationEnabled = Convert.ToBoolean(value);
								break;

							case "search":
								guild.GuildSettings.Modules.SearchEnabled = Convert.ToBoolean(value);
								break;

							case "stats":
								guild.GuildSettings.Modules.StatsEnabled = Convert.ToBoolean(value);
								break;
						}

						await database.UpdateGuildAsync(guild);
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

        [Command("configurechannel"), Summary("Some features require channels to be set"), RequireDatabase]
		[RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureChannel(string module, IChannel channel)
        {
            module = module.ToLowerInvariant();
            var modules = new Dictionary<string, string>()
            {
                //{"twitchnotif","twitchnotifchannel" },
                //{"twitch","twitchnotifchannel" },
                //{"twitchnotification","twitchnotifchannel" },
                //{"twitchnotifications","twitchnotifchannel" },
                //{"twitter","twitterlogchannel" },
                //{"twitterlog","twitterlogchannel" },
                {"userjoin","userjoinchan" },
                {"userjoined","userjoinchan" },
                {"userleave","userleavechan" },
                {"userleft","userleavechan" }
			};
            if (modules.ContainsKey(module)||modules.ContainsValue(module))
            {
				var guild = await database.GetGuildAsync(Context.Guild.Id);				

				if (guild!=null)
                {
					modules.TryGetValue(module, out string key);
					switch (key)
					{
						case "twitchnotifchannel":
							guild.TwitchNotifChannel = channel.Id;
							break;

						case "twitterlogchannel":
							guild.TwitterLogChannel = channel.Id;
							break;

						case "userjoinchan":
							guild.UserJoinChannel = channel.Id;
							break;

						case "userleavechan":
							guild.UserLeaveChannel = channel.Id;
							break;
					}
					await database.UpdateGuildAsync(guild);
                    await messageService.SendChannelAsync(Context.Channel, $"I set `{channel.Name}` as the channel for the `{module}` module");
                }
                else
                {
                    await database.InsertGuildAsync(Context.Guild);
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
