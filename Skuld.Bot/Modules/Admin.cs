﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Models;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Exceptions;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Pagination;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Extensions;
using Skuld.Services.Messaging.Extensions;
using Skuld.Services.Twitter;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;
using TwitchLib.Api.Interfaces;

namespace Skuld.Bot.Modules.Admin
{
	[Group, Name("Admin"), RequireRole(AccessLevel.ServerMod), RequireContext(ContextType.Guild), RequireEnabledModule]
	[Remarks("🛠 Administration Module")]
	public partial class AdminModule : InteractiveBase<ShardedCommandContext>
	{
		public SkuldConfig Configuration { get; set; }

		[
			Command("say"),
			Summary("Say something to a channel"),
			Usage("#general haha yes")
		]
		public async Task Say(ITextChannel channel, [Remainder] string message)
			=> await channel.SendMessageAsync(message).ConfigureAwait(false);

		#region Mute/Prune

		[
			Command("mute"),
			Summary("Mutes a user"),
			Usage("<@0>")
		]
		public async Task Mute(IUser usertomute)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var guild = Context.Guild;
			var roles = guild.Roles;
			var user = usertomute as IGuildUser;
			var channels = guild.TextChannels;

			var gld = await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false);

			try
			{
				if (gld.MutedRole == 0)
				{
					var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None, color: null, false, false).ConfigureAwait(false);
					foreach (var chan in channels)
					{
						await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan)).ConfigureAwait(false);
					}

					gld.MutedRole = role.Id;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await user.AddRoleAsync(role).ConfigureAwait(false);
					await EmbedExtensions.FromInfo($"{Context.User.Mention} just muted **{usertomute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
				else
				{
					var role = guild.GetRole(gld.MutedRole);
					await user.AddRoleAsync(role).ConfigureAwait(false);
					await EmbedExtensions.FromInfo($"{Context.User.Mention} just muted **{usertomute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
			catch
			{
				await EmbedExtensions.FromError($"Failed to give {usertomute.Username} the muted role, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("unmute"),
			Summary("Unmutes a user"),
			Usage("<@0>")
		]
		public async Task Unmute(IUser usertounmute)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var guild = Context.Guild;
			var user = usertounmute as IGuildUser;

			try
			{
				var dbGuild = await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false);

				if (dbGuild.MutedRole == 0)
				{
					await EmbedExtensions.FromError("Role doesn't exist, so I cannot unmute", Context).QueueMessageAsync(Context).ConfigureAwait(false);

					DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
				}
				else
				{
					var role = guild.GetRole(dbGuild.MutedRole);
					await user.RemoveRoleAsync(role).ConfigureAwait(false);
					await EmbedExtensions.FromInfo($"{Context.User.Mention} just unmuted **{usertounmute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
			catch
			{
				await EmbedExtensions.FromError($"Failed to remove the muted role from {usertounmute.Username}, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("prune"),
			Summary("Clean up your chat"),
			Usage("50", "50 <@0>")
		]
		public async Task Prune(short amount, IUser user = null)
		{
			if (amount < 0)
			{
				await EmbedExtensions.FromError($"{Context.User.Mention} Your amount `{amount}` is under 0.", Context).QueueMessageAsync(Context).ConfigureAwait(false);

				DogStatsd.Increment("commands.errors", 1, 1, new[] { "unm-precon" });
				return;
			}
			await Context.Message.DeleteAsync().ConfigureAwait(false);
			if (user is null)
			{
				var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync().ConfigureAwait(false);
				ITextChannel chan = (ITextChannel)Context.Channel;
				await chan.DeleteMessagesAsync(messages).ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{
						await EmbedExtensions.FromSuccess(":ok_hand: Done!", Context).QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 10).ConfigureAwait(false);
					}
				}).ConfigureAwait(false);
			}
			else
			{
				if (amount >= 100)
				{
					amount = Convert.ToInt16(100 * (100 % amount));
				}

				var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync().ConfigureAwait(false);
				var usermessages = messages.Where(x => x.Author.Id == user.Id);
				usermessages = usermessages.Take(amount);
				ITextChannel chan = (ITextChannel)Context.Channel;
				await chan.DeleteMessagesAsync(usermessages).ContinueWith(async x =>
				{
					if (x.IsCompleted)
					{
						await EmbedExtensions.FromSuccess(":ok_hand: Done!", Context).QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 10).ConfigureAwait(false);
					}
				}).ConfigureAwait(false);
			}
		}

		#endregion Mute/Prune

		#region Ban/Kick

		[
			Command("kick"),
			Summary("Kicks a user"),
			Alias("dab", "dabon"),
			RequireBotAndUserPermission(GuildPermission.KickMembers),
			Usage("@Person not following rules warning 2")
		]
		public async Task Kick(IGuildUser user, [Remainder] string reason = null)
		{
			try
			{
				var msg = $"You have been kicked from **{Context.Guild.Name}** by: {Context.User.Username}#{Context.User.Discriminator}";
				var guild = Context.Guild as IGuild;
				if (reason is null)
				{
					await user.KickAsync($"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
					if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) is null)
					{
						await EmbedExtensions.FromInfo($"Successfully kicked: `{user.Username}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}", Context).QueueMessageAsync(Context).ConfigureAwait(false);

						try
						{
							var dmchan = await user.CreateDMChannelAsync().ConfigureAwait(false);
							await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
						}
						catch { }
					}
				}
				else
				{
					msg += $" with reason:```\n{reason}```";
					await user.KickAsync(reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
					if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) is null)
					{
						await EmbedExtensions.FromInfo($"Successfully kicked: `{user}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}\nReason: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);

						try
						{
							var dmchan = await user.CreateDMChannelAsync().ConfigureAwait(false);
							await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
						}
						catch { }
					}
				}
			}
			catch
			{
				await EmbedExtensions.FromSuccess($"Couldn't kick {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("ban"),
			Summary("Bans a user"),
			Alias("naenae"),
			RequireBotAndUserPermission(GuildPermission.BanMembers),
			Usage("<@0> not following rules on multiple accounts")
		]
		public async Task Ban(IGuildUser user, [Remainder] string reason = null)
			=> await Ban(user, 7, reason).ConfigureAwait(false);

		[
			Command("ban"),
			Summary("Bans a user"),
			Alias("naenae"),
			RequireBotAndUserPermission(GuildPermission.BanMembers),
			Usage("<@0> 7 not following rules on multiple accounts")
		]
		public async Task Ban(IGuildUser user, int daystoprune = 7, [Remainder] string reason = null)
		{
			try
			{
				var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
				var guild = Context.Guild as IGuild;
				if (reason is null)
				{
					await Context.Guild.AddBanAsync(user, daystoprune, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
					if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) is null)
					{
						await EmbedExtensions.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						try
						{
							var dmchan = await user.CreateDMChannelAsync().ConfigureAwait(false);
							await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
						}
						catch { }
					}
				}
				else
				{
					msg += $" with reason:```\n{reason}```";
					await Context.Guild.AddBanAsync(user, daystoprune, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
					if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) is null)
					{
						await EmbedExtensions.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						try
						{
							var dmchan = await user.CreateDMChannelAsync().ConfigureAwait(false);
							await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
						}
						catch { }
					}
				}
			}
			catch
			{
				await EmbedExtensions.FromError($"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("hackban"),
			Summary("Hackbans a set of userids Must be in this format hackban [id1] [id2] [id3]"),
			RequireBotAndUserPermission(GuildPermission.BanMembers),
			Usage("1 2 3 4 5")
		]
		public async Task HackBan(params string[] ids)
		{
			if (ids.Any())
			{
				foreach (var id in ids)
					await Context.Guild.AddBanAsync(Convert.ToUInt64(id, CultureInfo.InvariantCulture)).ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"Banned IDs: {string.Join(", ", ids)}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await EmbedExtensions.FromError($"Couldn't parse list of ID's.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				DogStatsd.Increment("commands.errors", 1, 1, new[] { "parse-fail" });
			}
		}

		[
			Command("softban"),
			Summary("Softbans a user"),
			RequireBotAndUserPermission(GuildPermission.BanMembers),
			Usage("<@0> Controversial posts")
		]
		public async Task SoftBan(IUser user, [Remainder] string reason = null)
		{
			var newreason = $"Softban - Responsible Moderator: {Context.User.Username}#{Context.User.DiscriminatorValue}";
			if (reason is null)
			{
				await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
				await EmbedExtensions.FromSuccess($"Successfully softbanned: `{user.Username}#{user.Discriminator}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
			}
			else
			{
				newreason += " - Reason: " + reason;
				await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
				await EmbedExtensions.FromSuccess($"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
			}
		}

		#endregion Ban/Kick

		#region RoleManagement

		[
			Command("roleids"),
			Summary("Gets all role ids"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task GetRoleIds()
		{
			string lines = "";
			var guild = Context.Guild;
			var roles = guild.Roles;

			foreach (var item in roles)
			{
				lines = lines + $"{Convert.ToString(item.Id, CultureInfo.InvariantCulture)} - \"{item.Name}\"" + Environment.NewLine;
			}

			if (lines.Length > 2000)
			{
				var paddedlines = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

				var pages = paddedlines.PaginateCodeBlockList(25);

				foreach (var page in pages)
				{
					await page.QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
			else
			{
				await $"```cs\n{lines}```".QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("setjrole"),
			Summary("Allows a role to be auto assigned on userjoin"),
			RequireDatabase,
			Usage("@Member Role"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task AutoRole(IRole role = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var guild = Context.Guild;

			var gld = await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false);

			if (role is null)
			{
				if (gld.JoinRole != 0)
				{
					gld.JoinRole = 0;
					await Database.SaveChangesAsync().ConfigureAwait(false);

					if ((await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false)).JoinRole == 0)
					{
						await EmbedExtensions.FromSuccess($"Successfully removed the member join role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
					else
					{
						await EmbedExtensions.FromError($"Error Removing Join Role, reason unknown.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				}
			}
			else
			{
				var roleidprev = gld.JoinRole;

				gld.JoinRole = role.Id;
				await Database.SaveChangesAsync().ConfigureAwait(false);

				if ((await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false)).JoinRole != roleidprev)
				{
					await EmbedExtensions.FromSuccess($"Successfully set **{role.Name}** as the member join role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
				else
				{
					await EmbedExtensions.FromError($"Error Changing Join Role, reason unknown.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
		}

		[
			Command("autorole"),
			Summary("Get's guilds current autorole"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task AutoRole()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var jrole = (await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).JoinRole;

			if (jrole == 0)
			{
				await $"Currently, **{Context.Guild.Name}** has no auto role.".QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await $"**{Context.Guild.Name}**'s current auto role is `{Context.Guild.GetRole(jrole).Name}`".QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("persistentrole"),
			Summary("Toggle a role's persistent nature"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild),
			Usage("SuperAwesomeRole", "Super Duper Awesome Role")
		]
		public async Task PersistentRole([Remainder] IRole role)
		{
			using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

			PersistentRole prole = database.PersistentRoles.ToList().FirstOrDefault(x => x.RoleId == role.Id);

			if (prole is null)
			{
				var usersWithRole = await Context.Guild.GetUsersWithRoleAsync(role).ConfigureAwait(false);

				foreach (var userWithRole in usersWithRole)
				{
					database.PersistentRoles.Add(new PersistentRole
					{
						GuildId = Context.Guild.Id,
						RoleId = role.Id,
						UserId = userWithRole.Id
					});
				}

				await
					EmbedExtensions.FromSuccess(Context)
				.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				database.PersistentRoles.RemoveRange(database.PersistentRoles.ToList().Where(x => x.RoleId == role.Id && x.GuildId == Context.Guild.Id));

				await
					EmbedExtensions.FromSuccess(Context)
				.QueueMessageAsync(Context).ConfigureAwait(false);
			}

			await database.SaveChangesAsync().ConfigureAwait(false);
		}

		[
			Command("addassignablerole"),
			Summary("Adds a new self assignable role. Supported:cost=[cost] require-level=[level] require-role=[rolename/roleid/mention]"),
			Alias("asar"),
			Usage("\"Super Duper Role\"", "\"Super Duper Role\" require-level=25", "\"Super Duper Role\" require-level=25 cost=50"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task AddSARole(IRole role, [Remainder] GuildRoleConfig config = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var features = Database.Features.Find(Context.Guild.Id);

			if (config is null)
				config = new GuildRoleConfig();

			if (config.RequireLevel != 0 && !features.Experience)
			{
				await EmbedExtensions.FromError("Configuration Error!", $"Enable Experience module first by using `{(await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix}guild feature experience 1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			Database.IAmRoles.Add(new IAmRole
			{
				GuildId = Context.Guild.Id,
				LevelRequired = config.RequireLevel.GetValueOrDefault(0),
				Price = config.Cost.GetValueOrDefault(0),
				RequiredRoleId = config.RequiredRole is not null ?
									config.RequiredRole.Id :
									0,
				RoleId = role.Id
			});

			try
			{
				await Database.SaveChangesAsync().ConfigureAwait(false);
				await EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

				Log.Error("ASAR-CMD", ex.Message, Context, ex);
			}
		}

		[
			Command("addlevelrole"),
			Summary("Adds a new level role. Supported:automatic=[true/false] require-level=[level] require-role=[rolename/roleid/mention]"),
			Alias("alr"),
			Usage("\"Super Duper Role\"", "\"Super Duper Role\" require-level=25", "\"Super Duper Role\" require-level=25 automatic=true"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task AddLRole(IRole role, [Remainder] GuildRoleConfig config)
		{
			var levelReward = new LevelRewards
			{
				GuildId = Context.Guild.Id,
				LevelRequired = config.RequireLevel.GetValueOrDefault(0),
				RoleId = role.Id,
				Automatic = config.Automatic.GetValueOrDefault(false)
			};

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (Database.LevelRewards.Contains(levelReward))
			{
				await EmbedExtensions.FromError("Level reward already exists in this configuration", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			Database.LevelRewards.Add(levelReward);

			try
			{
				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromInfo($"Added new Level Reward with configuration `{config}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

				Log.Error("ALR-CMD", ex.Message, Context, ex);
			}
		}

		[
			Command("deletesr"),
			Summary("Removes a Self Assignable Role from the list"),
			Usage("Super Duper Role"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task DeleteSelfRole([Remainder] IRole role)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var r = Database.IAmRoles.FirstOrDefault(x => x.RoleId == role.Id);

			try
			{
				Database.IAmRoles.Remove(r);

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromInfo("Command Successful", $"Removed Self Assignable Role `{role}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError($"Command Error", ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

				Log.Error("DA-CMD", ex.Message, Context, ex);
			}
		}

		[
			Command("deletelr"),
			Summary("Removes a Level Grant Role from the list"),
			Alias("dlr"),
			Usage("Super Duper Role"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task DeleteLevelRole([Remainder] IRole role)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var r = Database.LevelRewards.FirstOrDefault(x => x.RoleId == role.Id);

			try
			{
				Database.LevelRewards.Remove(r);

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromInfo("Command Successful", $"Removed Self Assignable Role `{role}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError($"Command Error", ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

				Log.Error("DLR-CMD", ex.Message, Context, ex);
			}
		}

		[
			Command("updatelr"),
			Summary("Updates a Level Grant Role"),
			Alias("ulr"),
			Usage("\"Super Duper Role\" require-level=25", "\"Super Duper Role\" require-level=25 automatic=true"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task UpdateLevelRole(IRole role, [Remainder] GuildRoleConfig config)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var r = Database.LevelRewards.FirstOrDefault(x => x.RoleId == role.Id);

			r.Automatic = config.Automatic.GetValueOrDefault(r.Automatic);
			r.LevelRequired = config.RequireLevel.GetValueOrDefault(r.LevelRequired);

			try
			{
				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromInfo($"Added new Level Reward with configuration `{config}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

				Log.Error("ALR-CMD", ex.Message, Context, ex);
			}
		}

		[
			Command("modifyrole"),
			Summary("Modify a role's settings"),
			Usage("\"Super Duper Role\" hoisted=true", "\"Super Duper Role\" hoisted=true color=#ff0000"),
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task ModifyRole(IRole role, [Remainder] RoleConfig config = null)
		{
			if (config is null)
			{
				await RoleSettingsToEmbed(role)
					.AddFooter(Context)
					.AddAuthor()
					.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				return;
			}

			var currentUser = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
			var highestRole = currentUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault();

			if (role.Position > highestRole.Position)
			{
				await EmbedExtensions.FromError($"{role.Name} is higher than my current role, so can't modify it.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			int position = 0;

			if (config.Position.HasValue)
				position = config.Position.Value;
			else
				position = role.Position;

			try
			{
				await role.ModifyAsync(x =>
				{
					x.Position = position;
					x.Hoist = (bool)(config.Hoistable.HasValue ? config.Hoistable : role.IsHoisted);
					x.Mentionable = config.Mentionable ?? role.IsMentionable;
					x.Color = config.Color ?? role.Color;
				}).ConfigureAwait(false);

				await EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error(SkuldAppContext.GetCaller(), ex.Message, Context, ex);
				await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		private static EmbedBuilder RoleSettingsToEmbed(IRole role)
			=> new EmbedBuilder()
			.WithTitle(role.Name)
			.WithColor(role.Color)
			.AddInlineField("Hoisted", role.IsHoisted)
			.AddInlineField("Managed", role.IsManaged)
			.AddInlineField("Mentionable", role.IsMentionable)
			.AddInlineField("Position", role.Position)
			.AddInlineField("Color", role.Color.ToHex())
			.AddField("Created", role.CreatedAt);

		#endregion RoleManagement

		#region Prefix

		[
			Command("setprefix"),
			Summary("Sets the prefix, or resets on empty prefix"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild),
			Usage(">>")
		]
		public async Task SetPrefix(string prefix = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
			if (prefix is not null)
			{
				var oldprefix = gld.Prefix;

				gld.Prefix = prefix;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				if ((await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix != oldprefix)
					await EmbedExtensions.FromSuccess($"Successfully set `{prefix}` as the Guild's prefix", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				else
					await EmbedExtensions.FromError($":thinking: It didn't change. Probably because it is the same as the current prefix.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				gld.Prefix = Configuration.Prefix;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				if ((await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix == Configuration.Prefix)
					await EmbedExtensions.FromSuccess($"Successfully reset the Guild's prefix", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				else
					await EmbedExtensions.FromError($":thinking: It didn't change.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("resetprefix"),
			Summary("Resets prefix"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public async Task ResetPrefix()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

			if (gld is not null)
			{
				gld.Prefix = SkuldApp.MessageServiceConfig.Prefix;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await $"Reset the prefix back to `{SkuldApp.MessageServiceConfig.Prefix}`".QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await Database.InsertOrGetGuildAsync(Context.Guild, SkuldApp.MessageServiceConfig.Prefix, SkuldApp.MessageServiceConfig.MoneyName, SkuldApp.MessageServiceConfig.MoneyIcon).ConfigureAwait(false);
			}
		}

		#endregion Prefix

		[
			Name("Custom Commands"),
			Group("customcommands"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild)
		]
		public class CustomCommands : InteractiveBase<ShardedCommandContext>
		{
			[
				Command("add"),
				Summary("Adds a custom command"),
				Alias("addcommand"),
				Usage("beep boop")
			]
			public async Task AddCustomCommand(string name, [Remainder] string content)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (name.IsWebsite())
				{
					await EmbedExtensions.FromError("Commands can't be a url/website", Context).QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
					return;
				}
				if (name.Split(' ').Length > 1)
				{
					await EmbedExtensions.FromError("Commands can't contain a space", Context).QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
					return;
				}
				else
				{
					var cmdsearch = SkuldApp.CommandService.Search(Context, name);
					if (cmdsearch.Commands is not null)
					{
						await EmbedExtensions.FromError("This command is a global command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
					else
					{
						var cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.IsSameUpperedInvariant(name) && x.GuildId == Context.Guild.Id);

						if (cmd is not null)
						{
							await EmbedExtensions.FromInfo($"Custom command named `{name}` already exists, overwrite with new content? Y/N", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
							if (msg is not null)
							{
								if (msg.Content.IsSameUpperedInvariant("Y"))
								{
									var c = content;

									cmd.Content = content;

									await Database.SaveChangesAsync().ConfigureAwait(false);

									var cmd2 = Database.CustomCommands.FirstOrDefault(x => x.Name.IsSameUpperedInvariant(name) && x.GuildId == Context.Guild.Id);

									if (cmd2.Content != c)
									{
										await EmbedExtensions.FromInfo("Updated the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
									}
									else
									{
										await EmbedExtensions.FromError("Couldn't update the command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
									}
								}
							}
							else
							{
								await "Reply timed out, not updating.".QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
							}
							return;
						}
						else
						{
							Database.CustomCommands.Add(new CustomCommand
							{
								GuildId = Context.Guild.Id,
								Name = name,
								Content = content
							});
							await Database.SaveChangesAsync().ConfigureAwait(false);

							cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.IsSameUpperedInvariant(name) && x.GuildId == Context.Guild.Id);

							if (cmd is not null)
								await EmbedExtensions.FromInfo("Added the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							else
								await EmbedExtensions.FromError("Couldn't insert the command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
				}
			}

			[
				Command("delete"),
				Summary("Deletes a custom command"),
				Alias("deletecommand"),
				Usage("beep")
			]
			public async Task DeleteCustomCommand(string name)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (name.Split(' ').Length > 1)
				{
					await "Commands can't contain a space".QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else
				{
					await "Are you sure? Y/N".QueueMessageAsync(Context, type: Services.Messaging.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);

					var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
					if (msg is not null)
					{
						if (msg.Content.IsSameUpperedInvariant("y"))
						{
							Database.CustomCommands.Remove(Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.IsSameUpperedInvariant(name)));
							await Database.SaveChangesAsync().ConfigureAwait(false);

							if (Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.IsSameUpperedInvariant(name)) is null)
								await EmbedExtensions.FromInfo("Deleted the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							else
								await EmbedExtensions.FromError("Failed removing the command, try again", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
				}
			}

			[
				Command("change"),
				Summary("Updates a custom command"),
				Alias("changecommand"),
				Usage("beep bloop")
			]
			public async Task ChangeCustomCommand(string name, [Remainder] string content)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (name.Split(' ').Length > 1)
				{
					await "Commands can't contain a space".QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else
				{
					var cmd = Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.IsSameUpperedInvariant(name));

					cmd.Content = content;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					if (Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.IsSameUpperedInvariant(name)) is null)
						await EmbedExtensions.FromInfo("Updated the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					else
						await EmbedExtensions.FromError("Failed updating the command, try again", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
		}

		[
			Name("Experience"),
			Group("levels"),
			RequireUserPermission(GuildPermission.ManageGuild),
			RequireDatabase,
			Remarks("🎚 Configure the Experience Module")
		]
		public class Levels : ModuleBase<ShardedCommandContext>
		{
			[
				Command("message"),
				Summary("Sets the level up message, -u says the users name, -m mentions the user, -l shows the level they achieved -ud shows Username With Discriminator -jl shows the jumplink of the message if from text"),
				Usage("WOW! -m is now level -l!!!")
			]
			public async Task SetLevelUp([Remainder] string message)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				var oldmessg = gld.LeaveMessage;

				gld.LevelUpMessage = message;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var ngld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				if (ngld.LevelUpMessage != oldmessg)
				{
					await EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}

			[
				Command("notification"),
				Summary("Sets the levelup notification"),
				Alias("levelnotif"),
				Usage("#channel")
			]
			public async Task ConfigureLevelNotif(LevelNotification level)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				var old = gld.LevelNotification;

				gld.LevelNotification = level;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var ngld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				if (ngld.LevelNotification != old)
				{
					await EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}

			[
				Command("modifier"),
				Summary("Sets the XP Modifier"),
				Usage("2")
			]
			public async Task ConfigureXPRate(double rate)
			{
				if (rate > 3.0)
				{
					await EmbedExtensions.FromError("XP Modifier Invalid", "Above allowed rate, ensure it falls between 0.25 & 3.0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else if (rate < 0.25)
				{
					await EmbedExtensions.FromError("XP Modifier Invalid", "Below allowed rate, ensure it falls between 0.25 & 3.0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}

				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				var old = gld.XPModifier;

				gld.XPModifier = rate;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var ngld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				if (ngld.XPModifier != old)
				{
					await EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}
		}

		[
			Name("Server"),
			Group("guild"),
			RequireUserPermission(GuildPermission.ManageGuild),
			Remarks("🛠 Server management commands"),
			RequireDatabase
		]
		public class GuildManagement : ModuleBase<ShardedCommandContext>
		{
			public SkuldConfig Configuration { get; set; }

			[
				Command("money"),
				Summary("Set's the guilds money name or money icon"),
				Usage("🅱 bucks")
			]
			public async Task GuildMoney(Emoji icon = null, [Remainder] string name = null)
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();
				var guild = await database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				if (icon is null && name is null)
				{
					guild.MoneyIcon = SkuldApp.MessageServiceConfig.MoneyIcon;
					guild.MoneyName = SkuldApp.MessageServiceConfig.MoneyName;

					await database.SaveChangesAsync().ConfigureAwait(false);

					await EmbedExtensions.FromSuccess($"Reset the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);

					return;
				}

				if (icon is not null && name is null)
				{
					await EmbedExtensions.FromError($"Parameter \"{nameof(name)}\" needs a value", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}

				guild.MoneyIcon = icon.ToString();
				guild.MoneyName = name;

				await database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"Set the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			[
				Command("feature"),
				Summary("Configures guild features"),
				Usage("levels 1", "pinning 0")
			]
			public async Task ConfigureGuildFeatures(string module = null, int? value = null)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();
				var features = Database.Features.Find(Context.Guild.Id);

				if (!string.IsNullOrEmpty(module) && value.HasValue)
				{
					if (value > 1)
					{
						await EmbedExtensions.FromError("Value over max limit: `1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						return;
					}
					if (value < 0)
					{
						await EmbedExtensions.FromError("Value under min limit: `0`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						return;
					}
					else
					{
						module = module.ToLowerInvariant();
						var settings = new Dictionary<string, string>()
						{
							{"pinning", "pinning" },
							{"levels", "experience" },
							{"starboard", "starboard" },
							{"stackroles", "rolestack" },
							{"rolestacking", "rolestack" },
							{"stackingroles", "rolestack" }
						};
						if (settings.ContainsKey(module) || settings.ContainsValue(module))
						{
							var prev = features;

							if (features is not null)
							{
								var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);

								switch (setting.Value)
								{
									case "pinning":
										features.Pinning = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
										break;

									case "experience":
										features.Experience = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
										break;
									case "starboard":
										features.Starboard = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
										break;
									case "rolestack":
										features.StackingRoles = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
										break;
								}

								await Database.SaveChangesAsync().ConfigureAwait(false);

								if (value == 0) await $"I disabled the `{module}` feature".QueueMessageAsync(Context).ConfigureAwait(false);
								else await $"I enabled the `{module}` feature".QueueMessageAsync(Context).ConfigureAwait(false);
							}
						}
						else
						{
							string modulelist = "";
							foreach (var mod in settings) modulelist += mod.Key + " (" + mod.Value + ")" + ", ";

							modulelist = modulelist.Remove(modulelist.Length - 2);

							await EmbedExtensions.FromError($"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
				}
				else if (string.IsNullOrEmpty(module) && !value.HasValue)
				{
					await EmbedExtensions.FromMessage("Guild Features", "Here's the current status of features", Context)
									.AddInlineField("Levels", features.Experience ? "✅" : "❎")
									.AddInlineField("Pinning", features.Pinning ? "✅" : "❎")
									.QueueMessageAsync(Context)
									.ConfigureAwait(false);
				}
				else
				{
					throw new CommandExecutionException($"Parameter \"{nameof(value)}\" is empty, please provide a value");
				}
			}

			[
				Command("module"),
				Summary("Configures guild modules"),
				Usage("fun 0")
			]
			public async Task ConfigureGuildModules(string module, int value)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (value > 1)
				{
					await EmbedExtensions.FromError("Value over max limit: `1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				if (value < 0)
				{
					await EmbedExtensions.FromError("Value under min limit: `0`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else
				{
					module = module.ToLowerInvariant();

					var gldmods = Database.Modules.Find(Context.Guild.Id);

					string[] modules = null;

					List<string> mods = new();

					foreach (var mod in SkuldApp.CommandService.Modules.ToArray())
					{
						mods.Add(mod.Name.ToLowerInvariant());
					}

					modules = mods.ToArray();

					if (modules.Contains(module))
					{
						if (gldmods is not null)
						{
							switch (module)
							{
								case "accounts":
									gldmods.Accounts = Convert.ToBoolean(value);
									break;

								case "actions":
									gldmods.Actions = Convert.ToBoolean(value);
									break;

								case "admin":
									gldmods.Admin = Convert.ToBoolean(value);
									break;

								case "fun":
									gldmods.Fun = Convert.ToBoolean(value);
									break;

								case "gambling":
									gldmods.Gambling = Convert.ToBoolean(value);
									break;

								case "information":
									gldmods.Information = Convert.ToBoolean(value);
									break;

								case "lewd":
									gldmods.Lewd = Convert.ToBoolean(value);
									break;

								case "search":
									gldmods.Search = Convert.ToBoolean(value);
									break;

								case "space":
									gldmods.Space = Convert.ToBoolean(value);
									break;

								case "stats":
									gldmods.Stats = Convert.ToBoolean(value);
									break;

								case "weeb":
									gldmods.Weeb = Convert.ToBoolean(value);
									break;

								case "theplace":
									gldmods.ThePlace = Convert.ToBoolean(value);
									break;
							}

							await Database.SaveChangesAsync().ConfigureAwait(false);
							if (value == 0) await EmbedExtensions.FromSuccess($"I disabled the `{module}` module", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							else await EmbedExtensions.FromSuccess($"I enabled the `{module}` module", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						}
					}
					else
					{
						string modulelist = string.Join(", ", modules);
						await EmbedExtensions.FromError($"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				}
			}

			[
				Command("channel"),
				Summary("Some features require channels to be set"),
				Usage("join #userlog", "leave #userlog")
			]
			public async Task ConfigureChannel(string module, ITextChannel channel = null)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				module = module.ToLowerInvariant();
				var modules = new Dictionary<string, string>()
				{
					{"join", "userjoinchan" },
					{"userjoin","userjoinchan" },
					{"userjoined","userjoinchan" },
					{"leave", "userleavechan" },
					{"userleave","userleavechan" },
					{"userleft","userleavechan" },
					{"level", "levels" },
					{"experience", "levels" },
					{"levels", "levels" }
				};
				if (modules.ContainsKey(module) || modules.ContainsValue(module))
				{
					var guild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

					if (guild is not null)
					{
						modules.TryGetValue(module, out string key);
						switch (key)
						{
							case "userjoinchan":
								if (channel is not null)
								{
									guild.JoinChannel = channel.Id;
								}
								else
								{
									guild.JoinChannel = 0;
								}
								break;

							case "userleavechan":
								if (channel is not null)
								{
									guild.LeaveChannel = channel.Id;
								}
								else
								{
									guild.LeaveChannel = 0;
								}
								break;
							case "levels":
								if (channel is not null)
								{
									guild.LevelUpChannel = channel.Id;
								}
								else
								{
									guild.LevelUpChannel = 0;
								}
								break;
						}
						await Database.SaveChangesAsync().ConfigureAwait(false);

						await
							EmbedExtensions.FromSuccess(
								$"I set {channel.Mention} as the channel for the `{module}` module",
								Context
							).QueueMessageAsync(Context)
						.ConfigureAwait(false);
					}
					else await Database.InsertOrGetGuildAsync(Context.Guild, Configuration.Prefix, SkuldApp.MessageServiceConfig.MoneyName, SkuldApp.MessageServiceConfig.MoneyIcon).ConfigureAwait(false);
				}
				else
				{
					string modulelist = string.Join(", ", modules);
					modulelist = modulelist.Remove(modulelist.Length - 2);

					await
						EmbedExtensions.FromError(
							$"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}",
							Context
						).QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}
		}

		[
			Name("Welcome"),
			Group("welcome"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild),
			Remarks("👋 Welcome module")
		]
		public class Welcome : ModuleBase<ShardedCommandContext>
		{
			//Current Channel
			[
				Command("set"),
				Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots) -ud shows Username With Discriminator"),
				Usage("Welcome to the server -m! We hope you enjoy your stay!!!")
			]
			public async Task SetWelcome([Remainder] string welcome)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				var oldmessage = gld.JoinMessage;

				gld.JoinChannel = Context.Channel.Id;
				gld.JoinMessage = welcome;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Set Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			//Set Channel
			[
				Command("set"),
				Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots) -ud shows Username With Discriminator"),
				Usage("#userlog Welcome to the server -m! We hope you enjoy your stay!!!")
			]
			public async Task SetWelcome(ITextChannel channel, [Remainder] string welcome)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				var oldmessage = gld.JoinMessage;
				var oldchannel = gld.JoinChannel;

				gld.JoinChannel = channel.Id;
				gld.JoinMessage = welcome;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var ngld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Set Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			//Deletes
			[
				Command("unset"),
				Summary("Clears the welcome message"),
				Alias("clearwelcome", "unsetjoin", "delete", "remove", "clear")
			]
			public async Task SetWelcome()
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.JoinChannel = 0;
				gld.JoinMessage = "";
				gld.JoinImage = false;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var ngld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Cleared Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			[
				Command("image"),
				Summary("Enables/Disables the join image")
			]
			public async Task SetWelcomeImage(bool setImage)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.JoinImage = setImage;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"{(setImage ? "Enabled" : "Disabled")} welcome image!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Name("Leave"),
			Group("leave"),
			RequireDatabase,
			RequireUserPermission(GuildPermission.ManageGuild),
			Remarks("👋 Leave module")
		]
		public class Leave : ModuleBase<ShardedCommandContext>
		{
			//Set Channel
			[
				Command("set"),
				Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots) -ud shows Username With Discriminator"),
				Usage("#userlog Awww, how sad, -u just departed from the server.")
			]
			public async Task SetLeave(ITextChannel channel, [Remainder] string leave)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.LeaveChannel = channel.Id;
				gld.LeaveMessage = leave;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			//Current Channel
			[
				Command("set"),
				Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots) -ud shows Username With Discriminator"),
				Usage("Awww, how sad, -u just departed from the server.")
			]
			public async Task SetLeave([Remainder] string leave)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.LeaveChannel = Context.Channel.Id;
				gld.LeaveMessage = leave;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			//Deletes
			[
				Command("unset"),
				Summary("Clears the leave message"),
				Alias("unsetleave", "clear", "delete", "remove", "clearleave")
			]
			public async Task SetLeave()
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.LeaveChannel = 0;
				gld.LeaveMessage = "";

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			[
				Command("image"),
				Summary("Enables/Disables the leave image")
			]
			public async Task SetLeaveImage(bool setImage)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.LeaveImage = setImage;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"{(setImage ? "Enabled" : "Disabled")} leave image!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Name("Starboard"),
			Group("starboard"),
			RequireDatabase,
			RequireRole(AccessLevel.ServerMod),
			Remarks("⭐ Configure Starboard Module")
		]
		public class StarboardManagement : ModuleBase<ShardedCommandContext>
		{
			const string Key = "Starboard";

			[Command("toggle")]
			public async Task ToggleStarboard()
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var feats = Database.Features.Find(Context.Guild.Id);

				feats.Starboard = !feats.Starboard;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				if (feats.Starboard)
				{
					await
						EmbedExtensions.FromSuccess(Key,
							"Enabled the starboard feature",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				else
				{
					await
						EmbedExtensions.FromSuccess(Key,
							"Disabled the starboard feature",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}

			[Command("nsfw")]
			public async Task ToggleNSFW()
			{
				var guild = Context.Guild as IGuild;
				var botUser = await guild.GetCurrentUserAsync().ConfigureAwait(false);

				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
				var chan = await guild.GetTextChannelAsync(gld.StarboardChannel).ConfigureAwait(false);
				var overwrites = chan.GetPermissionOverwrite(Context.Client.CurrentUser);

				bool previousValue = chan.IsNsfw;
				if (botUser.GetPermissions(chan).ManageChannel || (overwrites.HasValue && overwrites.Value.ManageChannel == PermValue.Allow))
				{
					await chan.ModifyAsync(x => x.IsNsfw = !chan.IsNsfw).ConfigureAwait(false);
					await EmbedExtensions.FromInfo(Key, $"I changed {chan.Mention} into a {(previousValue ? "SFW" : "NSFW")} Channel", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
				else
				{
					await EmbedExtensions.FromInfo(Key, $"I can't turn {chan.Mention} into a {(previousValue ? "SFW" : "NSFW")} Channel", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
			}

			[
				Command("channel"),
				RequireFeature(GuildFeature.Starboard),
				Usage("#superawesomestarboard")
			]
			public async Task StarboardChannel([Remainder] ITextChannel channel = null)
			{
				var guild = (Context.Guild as IGuild);
				var botUser = await guild.GetCurrentUserAsync().ConfigureAwait(false);

				if (channel is null) channel = Context.Channel as ITextChannel;

				if (botUser.GetPermissions(channel).RawValue < DiscordUtilities.RequiredForStarboard.RawValue)
				{
					await
						EmbedExtensions.FromError(Key,
							$"Cannot use {channel.Mention} as the starboard channel, I need the permissions `READ_MESSAGES, EMBED_LINKS, ADD_REACTIONS`",
							Context)
					.QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}

				await channel.SendMessageAsync($"{Context.User.Mention} This channel will be used for the starboard").ConfigureAwait(false);

				{
					using var Database = new SkuldDbContextFactory().CreateDbContext();

					var gld = await Database.InsertOrGetGuildAsync(guild).ConfigureAwait(false);
					gld.StarboardChannel = channel.Id;

					await Database.SaveChangesAsync().ConfigureAwait(false);
				}
			}

			[
				Name("Starboard Emotes"),
				Group("emote"),
				RequireFeature(GuildFeature.Starboard)
			]
			public class StarboardEmote : ModuleBase<ShardedCommandContext>
			{
				[Command("")]
				public async Task StarboardEmoji()
					=> await StarboardEmoji("⭐").ConfigureAwait(false);

				[Command("")]
				[Usage("⭐")]
				public async Task StarboardEmoji(Emote emote)
					=> await StarboardEmoji(emote.ToString()).ConfigureAwait(false);

				[Command("")]
				[Usage(":superawesomestar:")]
				public async Task StarboardEmoji(Emoji emote)
					=> await StarboardEmoji(emote.ToString()).ConfigureAwait(false);

				public async Task StarboardEmoji(string emote)
				{
					using var Database = new SkuldDbContextFactory().CreateDbContext();
					var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

					gld.StarEmote = emote;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await
						EmbedExtensions.FromSuccess(
							Key,
							$"Successfully set {emote} as the server's starboard emote",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}

			[
				Name("Starboard Ranges"),
				Group("range"),
				RequireFeature(GuildFeature.Starboard)
			]
			public class StarboardRange : ModuleBase<ShardedCommandContext>
			{
				#region Range 10 -> 19
				[Command("1")]
				public async Task StarboardRange1()
					=> await StarboardRange1("🌟").ConfigureAwait(false);

				[Command("1")]
				[Usage("🌟")]
				public async Task StarboardRange1(Emote emote)
					=> await StarboardRange1(emote.ToString()).ConfigureAwait(false);

				[Command("1")]
				[Usage(":superawesomeemote:")]
				public async Task StarboardRange1(Emoji emote)
					=> await StarboardRange1(emote.ToString()).ConfigureAwait(false);

				public async Task StarboardRange1(string emote)
				{
					using var Database = new SkuldDbContextFactory().CreateDbContext();
					var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

					gld.StarRange1 = emote;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await
						EmbedExtensions.FromSuccess(
							Key,
							$"Successfully set {emote} as the server's starboard emote for the range range 10-19",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				#endregion
				#region Range 20 -> 29
				[Command("2")]
				public async Task StarboardRange2()
					=> await StarboardRange2("🌠").ConfigureAwait(false);

				[Command("2")]
				[Usage("🌠")]
				public async Task StarboardRange2(Emote emote)
					=> await StarboardRange2(emote.ToString()).ConfigureAwait(false);

				[Command("2")]
				[Usage(":superawesomeemote2:")]
				public async Task StarboardRange2(Emoji emote)
					=> await StarboardRange2(emote.ToString()).ConfigureAwait(false);

				public async Task StarboardRange2(string emote)
				{
					using var Database = new SkuldDbContextFactory().CreateDbContext();
					var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

					gld.StarRange2 = emote;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await
						EmbedExtensions.FromSuccess(
							Key,
							$"Successfully set {emote} as the server's starboard emote for the range 20-29",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				#endregion
				#region Range 30+
				[Command("3")]
				[Usage("")]
				public async Task StarboardRange3()
					=> await StarboardRange3("✨").ConfigureAwait(false);

				[Command("3")]
				[Usage("✨")]
				public async Task StarboardRange3(Emote emote)
					=> await StarboardRange3(emote.ToString()).ConfigureAwait(false);

				[Command("3")]
				[Usage(":superawesomeemote3:")]
				public async Task StarboardRange3(Emoji emote)
					=> await StarboardRange3(emote.ToString()).ConfigureAwait(false);

				public async Task StarboardRange3(string emote)
				{
					using var Database = new SkuldDbContextFactory().CreateDbContext();
					var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

					gld.StarRange3 = emote;

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await
						EmbedExtensions.FromSuccess(
							Key,
							$"Successfully set {emote} as the server's starboard emote for range 30+",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				#endregion
			}

			[
				Command("selfstar"),
				RequireFeature(GuildFeature.Starboard),
				Usage("true")
			]
			public async Task SelfStar(bool value)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();
				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.SelfStarring = value;

				if (value)
				{
					await
						EmbedExtensions.FromSuccess(Key,
							"Users can self star",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				else
				{
					await
						EmbedExtensions.FromSuccess(Key,
							"Users can not self star",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("starsrequired"),
				RequireFeature(GuildFeature.Starboard),
				Usage("5")
			]
			public async Task StarsRequired(ushort requiredStars)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();
				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.StarReactAmount = requiredStars;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Key,
						$"{requiredStars.ToFormattedString()} is required to get onto the starboard",
						Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("removeminimum"),
				RequireFeature(GuildFeature.Starboard),
				Usage("3")
			]
			public async Task RemoveMinimum(ushort minimumStars)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();
				var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.StarRemoveAmount = minimumStars;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Key,
						$"{minimumStars.ToFormattedString()} is required to removed from the starboard",
						Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("blacklist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("<@0>")
			]
			public async Task BlacklistUser([Remainder] IGuildUser user)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardBlacklist.Any(x => x.TargetId == user.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"{user.Mention} is already blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardBlacklist.Add(new StarboardConfigurable
					{
						TargetId = user.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"{user.Mention} is now blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("blacklist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("userlog")
			]
			public async Task BlacklistChannel([Remainder] ITextChannel channel)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardBlacklist.Any(x => x.TargetId == channel.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"{channel.Mention} is already blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardBlacklist.Add(new StarboardConfigurable
					{
						TargetId = channel.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"{channel.Mention} is now blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("blacklist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("Information")
			]
			public async Task BlacklistCategory([Remainder] ICategoryChannel category)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardBlacklist.Any(x => x.TargetId == category.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"Category {category.Name} is already blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardBlacklist.Add(new StarboardConfigurable
					{
						TargetId = category.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"Category {category.Name} is now blacklisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("whitelist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("<@0>")
			]
			public async Task WhitelistUser([Remainder] IGuildUser user)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardWhitelist.Any(x => x.TargetId == user.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"{user.Mention} is already whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardWhitelist.Add(new StarboardConfigurable
					{
						TargetId = user.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"{user.Mention} is now whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("whitelist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("userlog")
			]
			public async Task WhitelistChannel([Remainder] ITextChannel channel)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardWhitelist.Any(x => x.TargetId == channel.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"{channel.Mention} is already whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardWhitelist.Add(new StarboardConfigurable
					{
						TargetId = channel.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"{channel.Mention} is now whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			[
				Command("whitelist"),
				RequireFeature(GuildFeature.Starboard),
				Usage("Information")
			]
			public async Task WhitelistCategory([Remainder] ICategoryChannel category)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (Database.StarboardWhitelist.Any(x => x.TargetId == category.Id))
				{
					await
						EmbedExtensions.FromError(Key,
							$"Category {category.Name} is already whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}
				else
				{
					Database.StarboardWhitelist.Add(new StarboardConfigurable
					{
						TargetId = category.Id,
						GuildId = Context.Guild.Id
					});

					await
						EmbedExtensions.FromSuccess(Key,
							$"Category {category.Name} is now whitelisted",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		[
			Name("Twitch"),
			Group("twitchstreams"),
			RequireUserPermission(GuildPermission.ManageGuild),
			RequireConfiguration(ConfigParam.Twitch),
			RequireService(typeof(ITwitchAPI)),
			Remarks("📡 Configure Twitch Notifications")
		]
		public class Twitch : InteractiveBase<ShardedCommandContext>
		{
			[
				Command("follow"),
				Summary("Follows a twitch streamer"),
				RequireDatabase,
				Usage("#superawesomeupdates skuldbot")
			]
			public async Task FollowTwitchUsers(IMessageChannel channel, params string[] users)
			{
				var twitchClient = SkuldApp.Services.GetRequiredService<ITwitchAPI>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				var usrs = await twitchClient.V5.Users.GetUsersByNameAsync(users.ToList());

				foreach (var usr in usrs.Matches)
				{
					database.TwitchFollows.Add(new TwitchFollow
					{
						ChannelId = channel.Id,
						GuildId = Context.Guild.Id,
						Streamer = usr.Name
					});
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("follow"),
				Summary("Follows a twitch streamer"),
				RequireDatabase,
				Usage("skuldbot")
			]
			public async Task FollowTwitchUsers(params string[] users)
			{
				var twitchClient = SkuldApp.Services.GetRequiredService<ITwitchAPI>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				var usrs = await twitchClient.V5.Users.GetUsersByNameAsync(users.ToList());

				foreach (var usr in usrs.Matches)
				{
					database.TwitchFollows.Add(new TwitchFollow
					{
						ChannelId = Context.Channel.Id,
						GuildId = Context.Guild.Id,
						Streamer = usr.Name
					});
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("unfollow"),
				Summary("Unfollows a twitch streamer"),
				RequireDatabase,
				Usage("#superawesomeupdates skuldbot")
			]
			public async Task UnfollowTwitchUsers(IMessageChannel channel, params string[] users)
			{
				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (var usr in users)
				{
					var found = await database.TwitchFollows.ToAsyncEnumerable().FirstOrDefaultAsync(x => x.ChannelId == channel.Id && usr.IsSameUpperedInvariant(x.Streamer));
					if (found is not null)
					{
						database.Remove(found);
					}
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("unfollow"),
				Summary("Unfollows a twitch streamer"),
				RequireDatabase,
				Usage("skuldbot")
			]
			public async Task UnfollowTwitchUsers(params string[] users)
			{
				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (var usr in users)
				{
					var found = await database.TwitchFollows.ToAsyncEnumerable().FirstOrDefaultAsync(x => x.ChannelId == Context.Channel.Id && usr.IsSameUpperedInvariant(x.Streamer));
					if (found is not null)
					{
						database.Remove(found);
					}
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("message"),
				Summary("Sets the notification message. Keys are: -name & -url"),
				RequireDatabase,
				Usage("@TwitchWatchers -name is now live! Check them out: -url")
			]
			public async Task SetTwitchMessage([Remainder] string message)
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.TwitchLiveMessage = message;

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[
			Name("Twitter"),
			Group("twitterlog"),
			RequireUserPermission(GuildPermission.ManageGuild),
			RequireConfiguration(ConfigParam.Twitter),
			Remarks("🐦 Configure Twitter Notifications")
		]
		public class Twitter : InteractiveBase<ShardedCommandContext>
		{
			[
				Command("follow"),
				Summary("Follows a twitter user"),
				RequireDatabase,
				Usage("#superawesomeupdates skuldbot")
			]
			public async Task FollowTwitterUsers(IMessageChannel channel, params string[] users)
			{
				var twitterClient = SkuldApp.Services.GetRequiredService<ITwitterClient>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (string user in users)
				{
					var usr = await twitterClient.UsersV2.GetUserByNameAsync(user);

					if (database.TwitterFollows.Any(x => x.TwitterAccId == long.Parse(usr.User.Id)))
					{
						continue;
					}
					else
					{
						database.TwitterFollows.Add(new GuildTwitterAccounts
						{
							ChannelId = channel.Id,
							GuildId = Context.Guild.Id,
							TwitterAccId = long.Parse(usr.User.Id),
							TwitterAccName = user.ToLowerInvariant()
						});
					}
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				//TwitterListener.RunAsync();

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("follow"),
				Summary("Follows a twitter user"),
				RequireDatabase,
				Usage("skuldbot")
			]
			public async Task FollowTwitterUsers(params string[] users)
			{
				var twitterClient = SkuldApp.Services.GetRequiredService<ITwitterClient>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (string user in users)
				{
					var usr = await twitterClient.UsersV2.GetUserByNameAsync(user);

					if (database.TwitterFollows.Any(x => x.TwitterAccId == long.Parse(usr.User.Id)))
					{
						continue;
					}
					else
					{
						database.TwitterFollows.Add(new GuildTwitterAccounts
						{
							ChannelId = Context.Channel.Id,
							GuildId = Context.Guild.Id,
							TwitterAccId = long.Parse(usr.User.Id),
							TwitterAccName = user.ToLowerInvariant()
						});
					}
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				//TwitterListener.Run();

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("unfollow"),
				Summary("Unfollows a twitter user"),
				RequireDatabase,
				Usage("#superawesomeupdates skuldbot")
			]
			public async Task UnfollowTwitterUsers(IMessageChannel channel, params string[] users)
			{
				var twitterClient = SkuldApp.Services.GetRequiredService<ITwitterClient>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (string user in users)
				{
					if (!database.TwitterFollows.Any(x => x.ChannelId == Context.Channel.Id && x.TwitterAccName == user.ToLowerInvariant()))
					{
						continue;
					}

					database.TwitterFollows.Remove(database.TwitterFollows
						.FirstOrDefault(x => x.GuildId == Context.Guild.Id &&
							x.ChannelId == channel.Id &&
							x.TwitterAccName == user.ToLowerInvariant())
					);
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				//TwitterListener.Run();

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("unfollow"),
				Summary("Unfollows a twitter user"),
				RequireDatabase,
				Usage("skuldbot")
			]
			public async Task UnfollowTwitterUsers(params string[] users)
			{
				var twitterClient = SkuldApp.Services.GetRequiredService<ITwitterClient>();

				using SkuldDbContext database = new SkuldDbContextFactory().CreateDbContext();

				foreach (string user in users)
				{
					if (!database.TwitterFollows.Any(x => x.ChannelId == Context.Channel.Id && x.TwitterAccName == user.ToLowerInvariant()))
					{
						continue;
					}

					database.TwitterFollows.Remove(database.TwitterFollows
						.FirstOrDefault(x => x.GuildId == Context.Guild.Id &&
							x.ChannelId == Context.Channel.Id &&
							x.TwitterAccName == user.ToLowerInvariant())
					);
				}

				await database.SaveChangesAsync().ConfigureAwait(false);

				//TwitterListener.Run();

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("message"),
				Summary("Sets the notification message. Keys are: -name & -url"),
				RequireDatabase,
				Usage("@TwitterStalkers New tweet from -name! Check it out: -url")
			]
			public async Task SetTwitterMessage([Remainder] string message)
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				gld.NewTweetMessage = message;

				await database.SaveChangesAsync().ConfigureAwait(false);

				await
					EmbedExtensions.FromSuccess(Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}
	}
}