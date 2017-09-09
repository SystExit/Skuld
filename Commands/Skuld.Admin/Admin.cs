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
            await MessageHandler.SendChannel(channel, message);

        [Command("roleids", RunMode = RunMode.Async), Summary("Gets all role ids")]
        public async Task GetRoleIds()
        {
            List<string[]> lines = new List<string[]>()
            {
                new string[] { "Role Name", "ID" }
            };
            var guild = Context.Guild;
            var roles = guild.Roles;
            foreach (var item in roles)
            {
                lines.Add(new string[] { "\"" + item.Name + "\"", Convert.ToString(item.Id) });
            }
            var padded = ConsoleUtils.PrettyLines(lines, 3);
            if(padded.Length>2000)
            {
                var paddedlines = padded.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var paddedlinesptone = paddedlines.Take(paddedlines.Length / 2);
                var paddedlinespttwo = paddedlines.Skip(paddedlines.Length / 2).Take(paddedlines.Length / 2);
                string paddedlineone = null, paddedlinetwo = null;
                foreach (var item in paddedlinesptone)
                    paddedlineone += item+"\n";
                foreach (var item in paddedlinespttwo)
                    paddedlinetwo += item+"\n";

                await MessageHandler.SendChannel(Context.Channel, "Pt 1/2```cs\n" + paddedlineone + "```");
                await MessageHandler.SendChannel(Context.Channel, "Pt 2/2```cs\n" + paddedlinetwo + "```");
            }
            else
                await MessageHandler.SendChannel(Context.Channel, "```cs\n" + padded + "```");
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
            var roleid = await SqlTools.GetSingleAsync(cmd);
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
                await SqlTools.InsertAsync(cmd);
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
            var roleid = await SqlTools.GetSingleAsync(cmd);
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
                        await MessageHandler.SendChannel(Context.Channel, ":ok_hand: Done!", 5000);
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
                        await MessageHandler.SendChannel(Context.Channel, ":ok_hand: Done!",5000);
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
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("hackban", RunMode = RunMode.Async), Summary("Hackbans a userid")]
        public async Task HackBan(ulong ID)
        {
            await Context.Guild.AddBanAsync(ID);
            await MessageHandler.SendChannel(Context.Channel, $"Banned ID: {ID}");
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("hackban", RunMode = RunMode.Async), Summary("Hackbans a set of userids Must be in this format hackban [id1],[id2],[id3]")]
        public async Task HackBan([Remainder]string IDs)
        {
            var ids = IDs.Split(',');
            foreach (var id in ids)
            {
                await Context.Guild.AddBanAsync(Convert.ToUInt64(id));
            }
            await MessageHandler.SendChannel(Context.Channel, $"Banned IDs: {IDs}");
        }
        [Command("softban", RunMode = RunMode.Async), Summary("Softbans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user)
        {
            var reason = "Softban - No Reason Given - Responsible Moderator: "+Context.User.Username+"#"+Context.User.DiscriminatorValue;
            var guild = Context.Guild;
            await guild.AddBanAsync(user, 7, reason);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}");
            await guild.RemoveBanAsync(user);
        }
        [Command("softban", RunMode = RunMode.Async), Summary("Softbans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user, [Remainder]string reason)
        {
            var newreason = "Softban - "+reason+" - Responsible Moderator: " + Context.User.Username + "#" + Context.User.DiscriminatorValue;
            var guild = Context.Guild;
            await guild.AddBanAsync(user, 7, newreason);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {newreason}");
            await guild.RemoveBanAsync(user);
        }

        [Command("setjrole", RunMode = RunMode.Async), Summary("Clears the autojoinrole")]
        public async Task RemoveAutoRole()
        {
            var guild = Context.Guild;
            var cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", guild.Id);
            var reader = await SqlTools.GetSingleAsync(cmd);
            if (reader != null)
            {
                cmd = new MySqlCommand("UPDATE guild SET autojoinrole = NULL WHERE ID = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
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
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                var reader = await SqlTools.GetSingleAsync(cmd);
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
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select autojoinrole from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", guild.Id);
                var reader = await SqlTools.GetSingleAsync(cmd);
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
            var reader = await SqlTools.GetSingleAsync(cmd);
                if (!string.IsNullOrEmpty(reader))
                    await MessageHandler.SendChannel(Context.Channel, $"**{guild.Name}**'s current auto role is `{guild.GetRole(Convert.ToUInt64(reader)).Name}`");
                else
                    await MessageHandler.SendChannel(Context.Channel, $"Currently, **{guild.Name}** has no auto role.");
            await SqlTools.getconn.CloseAsync();
        }

        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setprefix", RunMode = RunMode.Async), Summary("Sets the prefix")]
        public async Task SetPrefix(string prefix)
        {
            var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            string oldprefix = await SqlTools.GetSingleAsync(cmd);

            cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@prefix", prefix);
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select prefix from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
                var result = await SqlTools.GetSingleAsync(cmd);
                    if (result != oldprefix)
                        await MessageHandler.SendChannel(Context.Channel, $"Successfully set `{prefix}` as the Guild's prefix");
                    else
                        await MessageHandler.SendChannel(Context.Channel, $":thinking: It didn't change. Probably because it is the same as the current prefix.");
            });
        }

        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("resetprefix", RunMode = RunMode.Async), Summary("Resets the prefix")]
        public async Task ResetPrefix()
        {
            var cmd = new MySqlCommand("select prefix from guild where id = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            string oldprefix = await SqlTools.GetSingleAsync(cmd);

            cmd = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@prefix", Config.Load().Prefix);
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);

            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                cmd = new MySqlCommand("select prefix from guild where id = @guildid");
                cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
                var result = await SqlTools.GetSingleAsync(cmd);
                if (result != oldprefix)
                    await MessageHandler.SendChannel(Context.Channel, $"Successfully reset the Guild's prefix");
            });
        }

        //Set Channel
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome([Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await SqlTools.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Welcome message!");
            });
        }

        //Current Channel
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserJoinChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await SqlTools.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET joinmessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", welcome);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Welcome message!");
            });
        }

        //Deletes
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setwelcome", RunMode = RunMode.Async), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetWelcome()
        {
            var cmd = new MySqlCommand("UPDATE guild SET joinmessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Removed Welcome message!");
            });            
        }

        //Set Channel
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", channel.Id);
            await SqlTools.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Leave message!");
            });
        }

        //Current Channel
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setleave", RunMode = RunMode.Async), Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        public async Task SetLeave([Remainder]string leave)
        {
            var cmd = new MySqlCommand("UPDATE guild SET UserLeaveChan = @chanid WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@chanid", Context.Channel.Id);
            await SqlTools.InsertAsync(cmd);
            cmd = new MySqlCommand("UPDATE guild SET leavemessage = @mesg WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            cmd.Parameters.AddWithValue("@mesg", leave);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Set Leave message!");
            });
        }

        //Deletes
        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setleave", RunMode = RunMode.Async), Summary("Clears the leave message")]
        public async Task SetLeave()
        {
            var cmd = new MySqlCommand("UPDATE guild SET leavemessage = NULL WHERE ID = @guildid");
            cmd.Parameters.AddWithValue("@guildid", Context.Guild.Id);
            await SqlTools.InsertAsync(cmd).ContinueWith(async x =>
            {
                await MessageHandler.SendChannel(Context.Channel, $"Removed Leave message!");
            });
        }

        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("guildfeature", RunMode = RunMode.Async), Summary("Configures guild features")]
        public async Task ConfigureGuildFeatures(string module, int value)
        {
            if (value > 1) await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build());
            if (value < 0) await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build());
            else
            {
                module = module.ToLowerInvariant();
                Dictionary<string, string> settings = new Dictionary<string, string>()
                {
                    {"starboard","starboard" },
                    {"levels","experience" },
                    {"userjoin", "userjoinleave" },
                    {"userleave","userjoinleave" },
                    {"usermod", "usermodification" },
                    {"guildmod", "guildmodification" },
                    {"chanmod", "guildchannelmodification" },
                    {"rolemod", "guildrolemodification" }
                };
                if (settings.ContainsKey(module) || settings.ContainsValue(module))
                {
                    if(!String.IsNullOrEmpty(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildfeaturemodules` WHERE `ID` = "+Context.Guild.Id))))
                    {
                        var setting = settings.Where(x => x.Key == module || x.Value == module).FirstOrDefault();
                        await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `guildfeaturemodules` SET `{setting.Value}` = {value} WHERE `ID` = {Context.Guild.Id}"));
                        if (value == 0)
                            await MessageHandler.SendChannel(Context.Channel, $"I disabled the `{module}` feature");
                        else
                            await MessageHandler.SendChannel(Context.Channel, $"I enabled the `{module}` feature");
                    }
                    else
                    {
                        await SqlTools.InsertAdvancedSettings(false, Context.Guild as Discord.WebSocket.SocketGuild);
                    }
                }
                else
                {
                    string modulelist = "";
                    foreach (var mod in settings)
                        modulelist += mod.Key + " ("+mod.Value+")"+ ", ";
                    modulelist = modulelist.Remove(modulelist.Length - 2);
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
                }                    
            }
        }

        [RequireUserPermission(GuildPermission.Administrator), RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("guildmodule", RunMode = RunMode.Async), Summary("Configures guild modules")]
        public async Task ConfigureGuildModules(string module, int value)
        {
            if (value > 1) await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build());
            if (value < 0) await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build());
            else
            {
                module = module.ToLowerInvariant();
                string[] modules = { "accounts","actions","admin","fun","help","information","search","stats","ai" };
                if (modules.Contains(module))
                {
                    if(!String.IsNullOrEmpty(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildcommandmodules` WHERE `ID` = "+Context.Guild.Id))))
                    {
                        await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `guildcommandmodules` SET `{module}` = {value} WHERE `ID` = {Context.Guild.Id}"));
                        if(value == 0)
                            await MessageHandler.SendChannel(Context.Channel, $"I disabled the `{module}` module");
                        else
                            await MessageHandler.SendChannel(Context.Channel, $"I enabled the `{module}` module");
                    }
                    else
                    {
                        await SqlTools.InsertAdvancedSettings(false, Context.Guild as Discord.WebSocket.SocketGuild);
                    }
                }
                else
                {
                    string modulelist = string.Join(", ",modules);
                    modulelist = modulelist.Remove(modulelist.Length - 2);
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
                }
            }
        }

        [RequireUserPermission(GuildPermission.Administrator),RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("configurechannel", RunMode = RunMode.Async), Summary("Some features require channels to be set")]
        public async Task ConfigureChannel(string module, IChannel Channel)
        {
            module = module.ToLowerInvariant();
            Dictionary<string, string> modules = new Dictionary<string, string>()
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
                {"userleft","userleavechan" },
                {"starboard","starboardchannel" },
                {"starboardchan","starboardchannel" }
            };
            if (modules.ContainsKey(module)||modules.ContainsValue(module))
            {
                if (!String.IsNullOrEmpty(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `ID` from `guildcommandmodules` WHERE `ID` = " + Context.Guild.Id))))
                {
                    var setting = modules.Where(x => x.Key == module || x.Value == module).FirstOrDefault();
                    await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `guild` SET `{setting.Value}` = {Channel.Id} WHERE `ID` = {Context.Guild.Id}"));
                    await MessageHandler.SendChannel(Context.Channel, $"I set `{Channel.Name}` as the channel for the `{module}` module");
                }
                else
                {
                    await SqlTools.InsertAdvancedSettings(false, Context.Guild as Discord.WebSocket.SocketGuild);
                }
            }
            else
            {
                string modulelist = string.Join(", ", modules);
                modulelist = modulelist.Remove(modulelist.Length - 2);
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build());
            }
        }
    }
}
