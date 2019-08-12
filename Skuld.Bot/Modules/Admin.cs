using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Models.Discord;
using Skuld.Core.Models.Skuld;
using Skuld.Database;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Models;
using Skuld.Discord.Preconditions;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireRole(AccessLevel.ServerMod), RequireEnabledModule]
    public class Admin : InteractiveBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        private CommandService CommandService { get => MessageHandler.CommandService; }

        [Command("say"), Summary("Say something to a channel")]
        public async Task Say(ITextChannel channel, [Remainder]string message) => await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("roleids"), Summary("Gets all role ids")]
        public async Task GetRoleIds()
        {
            string lines = "";
            var guild = Context.Guild;
            var roles = guild.Roles;

            foreach (var item in roles)
            {
                lines = lines + $"{Convert.ToString(item.Id)} - \"{item.Name}\"" + Environment.NewLine;
            }

            if (lines.Length > 2000)
            {
                var paddedlines = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var pages = paddedlines.PaginateCodeBlockList();

                await PagedReplyAsync(pages, fromSourceUser: true);
            }
            else
            {
                await $"```cs\n{lines}```".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("mute"), Summary("Mutes a user")]
        [RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = guild.TextChannels;

            if (Context.DBGuild.MutedRole == 0)
            {
                var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None);
                foreach (var chan in channels)
                {
                    await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan));
                }
                Context.DBGuild.MutedRole = role.Id;
                var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
                if (resp.All(x => x.Successful))
                {
                    await user.AddRoleAsync(role);
                    await $"{Context.User.Mention} just muted **{usertomute.Username}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    var err = resp.FirstOrDefault(x => x.Error != null).Error;
                    if (err == null)
                        err = "Unknown Reason";
                    await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("MuteCMD", "Unsuccessful", LogSeverity.Error, new Exception(err)));
                }
            }
            else
            {
                var role = guild.GetRole(Context.DBGuild.MutedRole);
                await user.AddRoleAsync(role);
                await $"{Context.User.Mention} just muted **{usertomute.Username}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
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

            if (Context.DBGuild.MutedRole == 0)
            {
                await "Role doesn't exist, so I cannot unmute".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
            }
            else
            {
                var role = guild.GetRole(Context.DBGuild.MutedRole);
                await user.RemoveRoleAsync(role);
                await $"{Context.User.Mention} just unmuted **{usertounmute.Username}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
        }

        [Command("prune"), Summary("Cleans set amount of messages.")]
        [RequireBotAndUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(int amount, IUser user = null)
        {
            if (amount < 0)
            {
                await $"{Context.User.Mention} Your amount `{amount}` is under 0.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                DogStatsd.Increment("commands.errors", 1, 1, new[] { "unm-precon" });
                return;
            }
            await Context.Message.DeleteAsync();
            if (user == null)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
                ITextChannel chan = (ITextChannel)Context.Channel;
                await chan.DeleteMessagesAsync(messages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                    {
                        await ":ok_hand: Done!".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);
                    }
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
                    {
                        await ":ok_hand: Done!".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);
                    }
                });
            }
        }

        [Command("kick"), Summary("Kicks a user"), Alias("dab", "dabon")]
        [RequireBotAndUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been kicked from **{Context.Guild.Name}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await user.KickAsync($"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await $"Successfully kicked: `{user.Username}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        var dmchan = await user.GetOrCreateDMChannelAsync();
                        await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await user.KickAsync(reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await ($"Successfully kicked: `{user}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}\nReason: " + reason).QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync();
                            await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                        }
                        catch
                        { /*Can be Ignored lol*/ }
                    }
                }
            }
            catch
            {
                await $"Couldn't kick {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("ban"), Summary("Bans a user"), Alias("naenae")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(user, 7, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync();
                            await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, 7, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync();
                            await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await $"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("ban"), Summary("Bans a user"), Alias("naenae")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, int daystoprune = 7, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(user, daystoprune, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync();
                            await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, daystoprune, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}");
                    if (await guild.GetUserAsync(user.Id) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync();
                            await msg.QueueMessage(Discord.Models.MessageType.DMFail, user, Context.Channel);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await $"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("hackban"), Summary("Hackbans a set of userids Must be in this format hackban [id1] [id2] [id3]")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task HackBan(params string[] ids)
        {
            if (ids.Count() > 0)
            {
                foreach (var id in ids)
                    await Context.Guild.AddBanAsync(Convert.ToUInt64(id));

                await $"Banned IDs: {string.Join(", ", ids)}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                await $"Couldn't parse list of ID's.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                DogStatsd.Increment("commands.errors", 1, 1, new[] { "parse-fail" });
            }
        }

        [Command("softban"), Summary("Softbans a user")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user, [Remainder]string reason = null)
        {
            var newreason = $"Softban - Responsible Moderator: {Context.User.Username}#{Context.User.DiscriminatorValue}";
            if (reason == null)
            {
                await Context.Guild.AddBanAsync(user, 7, newreason);
                await $"Successfully softbanned: `{user.Username}#{user.Discriminator}`".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                await Context.Guild.RemoveBanAsync(user);
            }
            else
            {
                newreason += " - Reason: " + reason;
                await Context.Guild.AddBanAsync(user, 7, newreason);
                await $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                await Context.Guild.RemoveBanAsync(user);
            }
        }

        [Command("setjrole"), Summary("Allows a role to be auto assigned on userjoin"), RequireDatabase]
        public async Task AutoRole(IRole role = null)
        {
            var guild = Context.Guild;

            if (role == null)
            {
                if (Context.DBGuild.JoinRole != 0)
                {
                    Context.DBGuild.JoinRole = 0;
                    var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
                    if (resp.All(x => x.Successful))
                    {
                        await $"Successfully removed the member join role".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                    }
                    else
                    {
                        string msg = "";
                        foreach (var res in resp)
                            msg += res.Exception + "\n";
                        await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("AJClCmd", "Unsuccessful", LogSeverity.Error, new Exception(msg)));
                    }
                }
            }
            else
            {
                Context.DBGuild.JoinRole = role.Id;
                var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
                if (resp.All(x => x.Successful))
                {
                    await $"Successfully set **{role.Name}** as the member join role".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    string msg = "";
                    foreach (var res in resp)
                        msg += res.Exception + "\n";
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("AJClCmd", "Unsuccessful", LogSeverity.Error, new Exception(msg)));
                }
            }
        }

        [Command("autorole"), Summary("Get's guilds current autorole"), RequireDatabase]
        public async Task AutoRole()
        {
            if (Context.DBGuild.JoinRole == 0)
            {
                await $"Currently, **{Context.Guild.Name}** has no auto role.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                await $"**{Context.Guild.Name}**'s current auto role is `{Context.Guild.GetRole(Convert.ToUInt64(Context.DBGuild.JoinRole)).Name}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("setprefix"), Summary("Sets the prefix, or resets on empty prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix = null)
        {
            if (prefix != null)
            {
                var sguild = Context.DBGuild;

                var oldprefix = sguild.Prefix;
                sguild.Prefix = prefix;
                var resp = await DatabaseClient.UpdateGuildAsync(sguild);
                sguild = (await DatabaseClient.GetGuildAsync(sguild.ID)).Data as SkuldGuild;

                if (resp.All(x => x.Successful))
                {
                    if (sguild.Prefix != oldprefix)
                    {
                        await $"Successfully set `{prefix}` as the Guild's prefix".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                    }
                    else
                    {
                        await $":thinking: It didn't change. Probably because it is the same as the current prefix.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                else
                {
                    var err = resp.FirstOrDefault(x => x.Error != null).Error;
                    if (err == null)
                        err = "Unknown Reason";
                    await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
            else
            {
                Context.DBGuild.Prefix = Configuration.Discord.Prefix;

                var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);

                if (resp.All(x => x.Successful))
                {
                    await $"Successfully reset the Guild's prefix".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    var err = resp.FirstOrDefault(x => x.Error != null).Error;
                    if (err == null)
                        err = "Unknown Reason";
                    await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
        }

        //Set Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome([Remainder]string welcome)
        {
            var sguild = Context.DBGuild;
            sguild.UserJoinChannel = Context.Channel.Id;
            sguild.JoinMessage = welcome;
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await $"Set Welcome message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        //Current Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            var sguild = Context.DBGuild;
            sguild.UserJoinChannel = channel.Id;
            sguild.JoinMessage = welcome;
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await "Set Welcome message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        //Deletes
        [Command("unsetwelcome"), Summary("Clears the welcome message"), Alias("clearwelcome"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome()
        {
            var sguild = Context.DBGuild;
            sguild.UserJoinChannel = 0;
            sguild.JoinMessage = "";
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await "Cleared Welcome message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        //Set Channel
        [Command("setleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            var sguild = Context.DBGuild;
            sguild.UserLeaveChannel = channel.Id;
            sguild.LeaveMessage = leave;
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await "Set Leave message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        //Current Channel
        [Command("setleave"), Alias("clearleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave([Remainder]string leave)
        {
            var sguild = Context.DBGuild;
            sguild.UserLeaveChannel = Context.Channel.Id;
            sguild.LeaveMessage = leave;
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await "Cleared Leave message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        //Deletes
        [Command("unsetleave"), Summary("Clears the leave message"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave()
        {
            var sguild = Context.DBGuild;
            sguild.UserLeaveChannel = 0;
            sguild.LeaveMessage = "";
            var resp = await DatabaseClient.UpdateGuildAsync(sguild);
            if (resp.All(x => x.Successful))
            {
                await "Set Leave message!".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("setlevelupmessage"), Summary("Sets the level up message, -u says the users name, -m mentions the user, -l shows the level they achieved"), RequireDatabase]
        [RequireRole(AccessLevel.ServerMod)]
        public async Task SetLevelUp([Remainder]string message)
        {
            var gld = Context.DBGuild;
            gld.LevelUpMessage = message;
            var resp = await DatabaseClient.UpdateGuildAsync(gld);
            if (resp.All(x => x.Successful))
            {
                await "".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var err = resp.FirstOrDefault(x => x.Error != null).Error;
                if (err == null)
                    err = "Unknown Reason";
                await err.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("addcommand"), Summary("Adds a custom command"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCustomCommand(string name, [Remainder]string content)
        {
            if (name.IsWebsite())
            {
                await "Commands can't be a url/website".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);
                return;
            }
            if (name.Split(' ').Length > 1)
            {
                await "Commands can't contain a space".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);
                return;
            }
            else
            {
                var cmdsearch = CommandService.Search(Context, name);
                if (cmdsearch.Commands != null)
                {
                    await "The bot already has this command".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
                else
                {
                    var custcmdResp = await DatabaseClient.GetCustomCommandAsync(Context.Guild.Id, name);
                    if (custcmdResp.Successful)
                    {
                        await $"Custom command named `{name}` already exists, overwrite with new content? Y/N".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);

                        var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5));
                        if (msg != null)
                        {
                            if (msg.Content.ToLower() == "y")
                            {
                                var resp = await DatabaseClient.UpdateCustomCommand(Context.Guild.Id, name, content);

                                if (resp.Successful)
                                {
                                    await "Updated the command.".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                                }
                                else
                                {
                                    await "Couldn't update the command".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                                }
                            }
                        }
                        else
                        {
                            await "Reply timed out, not updating.".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);
                        }
                        return;
                    }
                    else
                    {
                        var resp = await DatabaseClient.InsertCustomCommandAsync(Context.Guild, name, content);

                        if (resp.Successful)
                        {
                            await "Added the command.".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        }
                        else
                        {
                            await "Couldn't insert the command".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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
                await "Commands can't contain a space".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                return;
            }
            else
            {
                await "Are you sure? Y/N".QueueMessage(Discord.Models.MessageType.Timed, Context.User, Context.Channel, null, null, 5);

                var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5));
                if (msg != null)
                {
                    if (msg.Content.ToLower() == "y")
                    {
                        await DatabaseClient.DropCustomCommand(Context.Guild, name);
                        await "Deleted the command.".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                    }
                }
            }
        }

        [Command("guild-feature"), Summary("Configures guild features"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildFeatures(string module, int value)
        {
            if (value > 1) await new EmbedBuilder { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            if (value < 0) await new EmbedBuilder { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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
                    var guild = Context.DBGuild;

                    if (guild.Modules != null)
                    {
                        var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);

                        switch (setting.Value)
                        {
                            case "pinning":
                                guild.Features.Pinning = Convert.ToBoolean(value);
                                break;

                            case "experience":
                                guild.Features.Experience = Convert.ToBoolean(value);
                                break;
                        }

                        var res = await DatabaseClient.UpdateGuildAsync(guild);
                        if (res.All(x => x.Successful))
                        {
                            if (value == 0) await $"I disabled the `{module}` feature".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                            else await $"I enabled the `{module}` feature".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        }
                        else
                        {
                            var msg = "";
                            foreach (var re in res)
                            {
                                if (!re.Successful)
                                    msg += re.Error + "\n";
                            }
                            Console.WriteLine(msg);
                        }
                    }
                }
                else
                {
                    string modulelist = "";
                    foreach (var mod in settings) modulelist += mod.Key + " (" + mod.Value + ")" + ", ";

                    modulelist = modulelist.Remove(modulelist.Length - 2);
                    await new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
        }

        [Command("levelchannel"), Summary("Sets the levelup channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureLevelChannel(IGuildChannel channel = null)
        {
            if(channel == null)
            {
                Context.DBGuild.LevelUpChannel = 0;
                var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
                if(resp.All(x=>x.Successful))
                {
                    await "Removed Levelup Channel".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    var reason = "";
                    foreach (var r in resp)
                    {
                        if (!r.Successful)
                        {
                            reason += r.Error + "\n";
                        }
                    }
                    await reason.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
            else
            {
                Context.DBGuild.LevelUpChannel = 0;
                var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
                if (resp.All(x => x.Successful))
                {
                    await $"Set Levelup Channel to <#{channel.Id}>".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    var reason = "";
                    foreach(var r in resp)
                    {
                        if(!r.Successful)
                        {
                            reason += r.Error + "\n";
                        }
                    }
                    await reason.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
        }

        [Command("levelnotification"), Summary("Sets the levelup notification")]
        [Alias("levelnotif")]
        public async Task ConfigureLevelNotif(LevelNotification level)
        {
            Context.DBGuild.LevelNotification = level;
            var resp = await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
            if (resp.All(x => x.Successful))
            {
                await $"Set Levelup Notification to {level.ToString()}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                var reason = "";
                foreach (var r in resp)
                {
                    if (!r.Successful)
                    {
                        reason += r.Error + "\n";
                    }
                }
                await reason.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("guild-module"), Summary("Configures guild modules"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildModules(string module, int value)
        {
            if (value > 1) await new EmbedBuilder { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            if (value < 0) await new EmbedBuilder { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            else
            {
                module = module.ToLowerInvariant();
                string[] modules = { "accounts", "actions", "admin", "fun", "information", "lewd", "search", "stats", "weeb" };
                if (modules.Contains(module))
                {
                    var guild = Context.DBGuild;
                    if (guild.Modules != null)
                    {
                        switch (module)
                        {
                            case "accounts":
                                guild.Modules.AccountsEnabled = Convert.ToBoolean(value);
                                break;

                            case "actions":
                                guild.Modules.ActionsEnabled = Convert.ToBoolean(value);
                                break;

                            case "admin":
                                guild.Modules.AdminEnabled = Convert.ToBoolean(value);
                                break;

                            case "fun":
                                guild.Modules.FunEnabled = Convert.ToBoolean(value);
                                break;

                            case "information":
                                guild.Modules.InformationEnabled = Convert.ToBoolean(value);
                                break;

                            case "lewd":
                                guild.Modules.LewdEnabled = Convert.ToBoolean(value);
                                break;

                            case "search":
                                guild.Modules.SearchEnabled = Convert.ToBoolean(value);
                                break;

                            case "space":
                                guild.Modules.SpaceEnabled = Convert.ToBoolean(value);
                                break;

                            case "stats":
                                guild.Modules.StatsEnabled = Convert.ToBoolean(value);
                                break;

                            case "weeb":
                                guild.Modules.WeebEnabled = Convert.ToBoolean(value);
                                break;
                        }

                        await DatabaseClient.UpdateGuildAsync(guild);
                        if (value == 0) await $"I disabled the `{module}` module".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        else await $"I enabled the `{module}` module".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                else
                {
                    string modulelist = string.Join(", ", modules);
                    await new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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
                {"userjoin","userjoinchan" },
                {"userjoined","userjoinchan" },
                {"userleave","userleavechan" },
                {"userleft","userleavechan" }
            };
            if (modules.ContainsKey(module) || modules.ContainsValue(module))
            {
                var guild = Context.DBGuild;

                if (guild != null)
                {
                    modules.TryGetValue(module, out string key);
                    switch (key)
                    {
                        case "userjoinchan":
                            guild.UserJoinChannel = channel.Id;
                            break;

                        case "userleavechan":
                            guild.UserLeaveChannel = channel.Id;
                            break;
                    }
                    await DatabaseClient.UpdateGuildAsync(guild);
                    await $"I set `{channel.Name}` as the channel for the `{module}` module".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else await DatabaseClient.InsertGuildAsync(Context.Guild.Id, Configuration.Discord.Prefix);
            }
            else
            {
                string modulelist = string.Join(", ", modules);
                modulelist = modulelist.Remove(modulelist.Length - 2);

                await new EmbedBuilder
                {
                    Title = "Error with command",
                    Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}",
                    Color = new Color(255, 0, 0) }
                .Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("resetprefix"), Summary("Resets prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetPrefix()
        {
            var sguild = Context.DBGuild;
            if (sguild != null)
            {
                sguild.Prefix = Configuration.Discord.Prefix;
                await DatabaseClient.UpdateGuildAsync(sguild);
                await $"Reset the prefix back to `{Configuration.Discord.Prefix}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                await DatabaseClient.InsertGuildAsync(Context.Guild.Id, Configuration.Discord.Prefix);
                await $"`{Context.Guild.Name}` doesn't exist in database, fixing.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Disabled]
        [Command("serverrole"), Summary("Adds a new self assignable role. Use: optable=[true/false] cost=[cost] level-grant=[true/false] require-level=[level] require-role=[rolename/roleid/mention]")]
        public async Task AddRole(IRole role, GuildRoleConfig config)
        {
            EventResult result = new EventResult();

            if(config.Optable)
            {
                result = await DatabaseClient.AddGuildAssignRoleAsync(Context.Guild, role.Id, config);
            }
            else if(config.LevelReward)
            {
                result = await DatabaseClient.AddGuildLevelRewardAsync(Context.Guild, role.Id, config);
            }
            else
            {
                await "Your role needs to either be optable or a level reward".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            if(result.Successful)
            {
                await "".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                await result.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Disabled]
        [Command("deleterole"), Summary("Removes a custom role")]
        public async Task DeleteRole(IRole role)
        {

        }

        [Disabled]
        [Command("deleterole"), Summary("Removes a custom role")]
        public async Task DeleteRole(ulong roleID)
        {
            if(Context.DBGuild.JoinableRoles.Where(x=>x == roleID).Count() > 0)
            {
                Context.DBGuild.JoinableRoles.Remove(roleID);
                await DatabaseClient.UpdateGuildAsync(Context.DBGuild);
            }
        }
    }
}