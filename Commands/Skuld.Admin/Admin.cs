using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Skuld.Models;
using Discord;
using MySql.Data.MySqlClient;

namespace Skuld.Commands
{
    [Group,Name("Admin"),RequireRolePrecondition(AccessLevel.ServerMod)]
    public class Admin : ModuleBase
    {
        [Command("say", RunMode = RunMode.Async), Summary("Say something to a channel")]
        public async Task Say(IMessageChannel channel, [Remainder] string message) =>
            await MessageHandler.SendChannel(Context.Channel, message);

        [Command("roleids", RunMode = RunMode.Async), Summary("Gets all role ids")]
        public async Task GetRoleIds()
        {
            List<string[]> lines = new List<string[]>();
            lines.Add(new string[] { "Role Name", "ID" });
            var guild = Context.Guild;
            var roles = guild.Roles;
            foreach (var item in roles)
            {
                lines.Add(new string[] { "\"" + item.Name + "\"", Convert.ToString(item.Id) });
            }
            var padded = ConsoleUtils.PrettyLines(lines, 3);
            await MessageHandler.SendChannel(Context.Channel,"```cs\n" + padded + "```");
        }

        [Command("mute", RunMode = RunMode.Async), Summary("Mutes a user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = await guild.GetTextChannelsAsync();
            var cmd = new MySqlCommand("SELECT MutedRole from guild WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var roleid = await Sql.GetSingleAsync(cmd);
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
                await Sql.InsertAsync(cmd);
                await user.AddRoleAsync(role);
                await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
            }
            else
            {
                var role = guild.GetRole(Convert.ToUInt64(roleid));
                await user.AddRoleAsync(role);
                await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} just muted **{usertomute.Username}**");
            }
        }
        [Command("unmute", RunMode = RunMode.Async), Summary("Unmutes a user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(IUser usertounmute)
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertounmute as IGuildUser;
            var channels = await guild.GetTextChannelsAsync();
            var cmd = new MySqlCommand("SELECT MutedRole from guild WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var roleid = await Sql.GetSingleAsync(cmd);
            if (String.IsNullOrEmpty(roleid))
            {
                await MessageHandler.SendChannel(Context.Channel, "Role doesn't exist, so I cannot unmute");
            }
            else
            {
                var role = guild.GetRole(Convert.ToUInt64(roleid));
                await user.RemoveRoleAsync(role);
                await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} just unmuted **{usertounmute.Username}**");
            }
        }

        [Command("pm", RunMode = RunMode.Async), Summary("PMs a user")]
        public async Task DM(IGuildUser user, [Remainder]string message)
        {
            string newmesg = $"**{Context.User.Username}#{Context.User.DiscriminatorValue}** says: {message}";
            var msg2del = Context.Message;
            var dmchannel = await user.GetOrCreateDMChannelAsync();
            await dmchannel.SendMessageAsync(newmesg);
            await msg2del.DeleteAsync();
        }

        [Command("prune", RunMode = RunMode.Async), Summary("Cleans set amount of messages.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(int amount)
        {
            if (amount < 0) { await MessageHandler.SendChannel(Context.Channel,$"{Context.User.Mention} Your amount `{amount}` is under 0."); }
            else
            {
                amount++;
                var messages = await Context.Channel.GetMessagesAsync(amount).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                        await MessageHandler.SendChannel(Context.Channel, ":ok_hand: Done!", null, 5000);
                });
            }
        }
        [Command("prune", RunMode = RunMode.Async), Summary("Cleans set amount of messages.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(IUser user, int amount)
        {
            if (amount < 0) { await MessageHandler.SendChannel(Context.Channel,$"{Context.User.Mention} Your amount `{amount}` is under 0."); }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(100).Flatten();
                var usermessages = messages.Where(x => x.Author.Id == user.Id);
                usermessages = usermessages.Take(amount);

                await Context.Channel.DeleteMessagesAsync(usermessages).ContinueWith(async x =>
                {
                    if(x.IsCompleted)
                        await MessageHandler.SendChannel(Context.Channel, ":ok_hand: Done!",null,5000);
                });
            }
        }

        [Command("kick", RunMode = RunMode.Async), Summary("Kicks a user")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user)
        {
            var guild = Context.Guild;
            try
            {
                var dmchan = await user.GetOrCreateDMChannelAsync();
                await dmchan.SendMessageAsync($"You have been kicked from **{Context.Guild}**");
            }
            catch { }
            await user.KickAsync("Responsible Moderator: "+Context.User.Username+"#"+Context.User.DiscriminatorValue);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully kicked: `{user.Username}#{user.Discriminator}`");
        }
        [Command("kick", RunMode = RunMode.Async), Summary("Kicks a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Kick(IGuildUser user, [Remainder]string reason)
        {
            var guild = Context.Guild;
            try
            {
                var dmchan = await user.GetOrCreateDMChannelAsync();
                await dmchan.SendMessageAsync($"You have been kicked from **{Context.Guild}** with reason:```\n{reason}```");
            }
            catch { }
            await user.KickAsync("Responsible Moderator: " + Context.User.Username + "#" + Context.User.DiscriminatorValue+" : "+reason);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully kicked: `{user.Username}#{user.Discriminator}`\nReason given: {reason}");
        }

        [Command("ban", RunMode = RunMode.Async), Summary("Bans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IUser user)
        {
            var guild = Context.Guild;
            try
            {
                var dmchan = await user.GetOrCreateDMChannelAsync();
                await dmchan.SendMessageAsync($"You have been banned from **{Context.Guild}**");
            }
            catch { }
            await guild.AddBanAsync(user, 7);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully banned: `{user.Username}#{user.Discriminator}`");
        }
        [Command("ban", RunMode = RunMode.Async), Summary("Bans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IUser user, [Remainder]string reason)
        {
            var guild = Context.Guild;
            try
            {
                var dmchan = await user.GetOrCreateDMChannelAsync();
                await dmchan.SendMessageAsync($"You have been banned from **{Context.Guild}** with reason:```\n{reason}```");
            }
            catch { }
            await guild.AddBanAsync(user, 7, reason);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully banned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}");
        }
        [Command("softban", RunMode = RunMode.Async), Summary("Softbans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user)
        {
            var reason = "Softban - No Reason Given - Responsible Moderator: "+Context.User.Username+"#"+Context.User.DiscriminatorValue;
            var guild = Context.Guild;
            try
            {
                await guild.AddBanAsync(user, 7, reason);
                await MessageHandler.SendChannel(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}");
                await guild.RemoveBanAsync(user);
            }
            catch(Exception ex)
            {
            }
        }
        [Command("softban", RunMode = RunMode.Async), Summary("Softbans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user, [Remainder]string reason)
        {
            var newreason = "Softban - "+reason+" - Responsible Moderator: " + Context.User.Username + "#" + Context.User.DiscriminatorValue;
            var guild = Context.Guild;
            try
            {
                await guild.AddBanAsync(user, 7, newreason);
                await MessageHandler.SendChannel(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {newreason}");
                await guild.RemoveBanAsync(user);
            }
            catch (Exception ex)
            {
            }
        }

        [Command("setjrole", RunMode = RunMode.Async), Summary("Clears the autojoinrole")]
        public async Task RemoveAutoRoll()
        {
            var guild = Context.Guild;
            var cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var reader = await Sql.GetSingleAsync(cmd);
            if (reader != null)
            {
                cmd = new MySqlCommand("UPDATE guild SET autojoinrole = NULL WHERE ID = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                await Sql.InsertAsync(cmd).ContinueWith(async x =>
                {
                    await MessageHandler.SendChannel(Context.Channel, $"Successfully removed the member join role");
                });
            }
        }
        [Command("setjrole", RunMode = RunMode.Async), Summary("Allows a role to be auto assigned on userjoin")]
        public async Task AutoRole(IRole role)
        {
            var guild = Context.Guild;
            var cmd = new MySqlCommand("UPDATE guild SET autojoinrole = @roleid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            cmd.Parameters.AddWithValue("@roleid", role.Id);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                var reader = await Sql.GetSingleAsync(cmd);
                    if (reader != null)
                        await MessageHandler.SendChannel(Context.Channel, $"Successfully set **{role.Name}** as the member join role");
            });
        }
        [Command("setjrole", RunMode = RunMode.Async), Summary("Allows a role to be auto assigned on userjoin")]
        public async Task AutoRole(ulong roleid)
        {
            var guild = Context.Guild;
            var role = guild.GetRole(roleid);
            var cmd = new MySqlCommand("UPDATE guild SET autojoinrole = @roleid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            cmd.Parameters.AddWithValue("@roleid", role.Id);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                var reader = await Sql.GetSingleAsync(cmd);
                if (reader != null)
                    await MessageHandler.SendChannel(Context.Channel, $"Successfully set **{role.Name}** as the member join role");
            });
        }
        [Command("autorole", RunMode = RunMode.Async), Summary("Get's guilds current autorole")]
        public async Task AutoRole()
        {
            IGuild guild = Context.Guild;
            var cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var reader = await Sql.GetSingleAsync(cmd);
                if (!string.IsNullOrEmpty(reader))
                    await MessageHandler.SendChannel(Context.Channel, $"**{guild.Name}**'s current auto role is `{guild.GetRole(Convert.ToUInt64(reader)).Name}`");
                else
                    await MessageHandler.SendChannel(Context.Channel, $"Currently, **{guild.Name}** has no auto role.");
            await Sql.getconn.CloseAsync();
        }

        [RequireRolePrecondition(AccessLevel.ServerAdmin)]
        [Command("setprefix", RunMode = RunMode.Async), Summary("Sets the prefix")]
        public async Task SetPrefix(string prefix)
        {
            var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            string oldprefix = await Sql.GetSingleAsync(cmd);

            cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@prefix", prefix);
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select prefix from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
                var result = await Sql.GetSingleAsync(cmd);
                    if (result != oldprefix)
                        await MessageHandler.SendChannel(Context.Channel, $"Successfully set `{prefix}` as the Guild's prefix");
                    else
                        await MessageHandler.SendChannel(Context.Channel, $":thinking: It didn't change. Probably because it is the same as the current prefix.");
            });
        }
        [RequireRolePrecondition(AccessLevel.ServerAdmin)]
        [Command("resetprefix", RunMode = RunMode.Async), Summary("Resets the prefix")]
        public async Task ResetPrefix()
        {
            var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            string oldprefix = await Sql.GetSingleAsync(cmd);

            cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@prefix", Config.Load().Prefix);
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);

            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select prefix from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
                var result = await Sql.GetSingleAsync(cmd);
                if (result != oldprefix)
                    await MessageHandler.SendChannel(Context.Channel, $"Successfully reset the Guild's prefix");
            });
        }
        
        //Set Channel
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome([Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await Sql.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Welcome message!");
            });
        }

        //Current Channel
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await Sql.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Welcome message!");
            });
        }

        //Deletes
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome()
        {
            var cmd = new MySqlCommand("UPDATE guild SET joinmessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Removed Welcome message!");
            });            
        }

        //Set Channel
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await Sql.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Leave message!");
            });
        }

        //Current Channel
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetLeave([Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await Sql.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Leave message!");
            });
        }

        //Deletes
        [Command("setleave", RunMode = RunMode.Async), Summary("Clears the leave message")]
        public async Task SetLeave()
        {
            var cmd = new MySqlCommand("UPDATE guild SET leavemessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await Sql.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Removed Leave message!");
            });
        }
    }
}
