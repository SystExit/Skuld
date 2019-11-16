using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Bot.Services;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
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
    public class Admin : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        private CommandService CommandService { get => MessageHandler.CommandService; }

        [Command("say"), Summary("Say something to a channel")]
        public async Task Say(ITextChannel channel, [Remainder]string message)
            => await channel.SendMessageAsync(message).ConfigureAwait(false);

        #region GeneralManagement
        [Command("guild-feature"), Summary("Configures guild features"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildFeatures(string module, int value)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (value > 1) await new EmbedBuilder { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            if (value < 0) await new EmbedBuilder { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            else
            {
                module = module.ToLowerInvariant();
                var settings = new Dictionary<string, string>()
                {
                    {"pinning", "pinning" },
                    {"levels", "experience" }
                };
                if (settings.ContainsKey(module) || settings.ContainsValue(module))
                {
                    var features = Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id);
                    var prev = features;

                    if (features != null)
                    {
                        var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);

                        switch (setting.Value)
                        {
                            case "pinning":
                                features.Pinning = Convert.ToBoolean(value);
                                break;

                            case "experience":
                                features.Experience = Convert.ToBoolean(value);
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
                    await new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                }
            }
        }

        [Command("guild-module"), Summary("Configures guild modules"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildModules(string module, int value)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (value > 1) await new EmbedBuilder { Description = "Value over max limit: `1`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);

            if (value < 0) await new EmbedBuilder { Description = "Value under min limit: `0`", Title = "ERROR With Command", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            else
            {
                module = module.ToLowerInvariant();

                var gldmods = Database.Modules.FirstOrDefault(x => x.Id == Context.Guild.Id);

                string[] modules = null;

                List<string> mods = new List<string>();

                foreach (var mod in CommandService.Modules.ToArray())
                {
                    mods.Add(mod.Name.ToLowerInvariant());
                }

                modules = mods.ToArray();

                if (modules.Contains(module))
                {
                    if (gldmods != null)
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
                        }

                        await Database.SaveChangesAsync().ConfigureAwait(false);
                        if (value == 0) await $"I disabled the `{module}` module".QueueMessageAsync(Context).ConfigureAwait(false);
                        else await $"I enabled the `{module}` module".QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    string modulelist = string.Join(", ", modules);
                    await new EmbedBuilder { Title = "Error with command", Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Color = new Color(255, 0, 0) }.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                }
            }
        }

        [Command("configurechannel"), Summary("Some features require channels to be set"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureChannel(string module, IChannel channel)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

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
                var guild = await Database.GetGuildAsync(Context.Guild);

                if (guild != null)
                {
                    modules.TryGetValue(module, out string key);
                    switch (key)
                    {
                        case "userjoinchan":
                            guild.JoinChannel = channel.Id;
                            break;

                        case "userleavechan":
                            guild.LeaveChannel = channel.Id;
                            break;
                    }
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await $"I set `{channel.Name}` as the channel for the `{module}` module".QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else await Database.InsertGuildAsync(Context.Guild, MessageHandler.cmdConfig.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon).ConfigureAwait(false);
            }
            else
            {
                string modulelist = string.Join(", ", modules);
                modulelist = modulelist.Remove(modulelist.Length - 2);

                await new EmbedBuilder
                {
                    Title = "Error with command",
                    Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}",
                    Color = new Color(255, 0, 0)
                }
                .Build().QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("guild-money"), Summary("Set's the guilds money name or money icon"), RequireDatabase]
        public async Task GuildMoney(Emoji icon = null, [Remainder]string name = null)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();
            var guild = await database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (icon == null && name == null)
            {
                guild.MoneyIcon = "💠";
                guild.MoneyName = "Diamonds";

                await database.SaveChangesAsync();

                await $"Reset the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`".QueueMessageAsync(Context);

                return;
            }

            if(icon != null && name == null)
            {
                await $"Parameter \"{nameof(name)}\" needs a value".QueueMessageAsync(Context, Discord.Models.MessageType.Failed);
                return;
            }

            guild.MoneyIcon = icon.ToString();
            guild.MoneyName = name;

            await database.SaveChangesAsync();

            await $"Set the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`".QueueMessageAsync(Context);
        }

        #endregion GeneralManagement

        #region Mute/Prune

        [Command("mute"), Summary("Mutes a user")]
        [RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = guild.TextChannels;

            var gld = await Database.GetGuildAsync(guild).ConfigureAwait(false);

            try
            {
                if (gld.MutedRole == 0)
                {
                    var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None).ConfigureAwait(false);
                    foreach (var chan in channels)
                    {
                        await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan)).ConfigureAwait(false);
                    }

                    gld.MutedRole = role.Id;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    await $"{Context.User.Mention} just muted **{usertomute.Username}**".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
                else
                {
                    var role = guild.GetRole(gld.MutedRole);
                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    await $"{Context.User.Mention} just muted **{usertomute.Username}**".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
            }
            catch
            {
                await $"Failed to give {usertomute.Username} the muted role, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            }
        }

        [Command("unmute"), Summary("Unmutes a user")]
        [RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(IUser usertounmute)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;
            var user = usertounmute as IGuildUser;

            try
            {
                var dbGuild = await Database.GetGuildAsync(guild);

                if (dbGuild.MutedRole == 0)
                {
                    await "Role doesn't exist, so I cannot unmute".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);

                    DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
                }
                else
                {
                    var role = guild.GetRole(dbGuild.MutedRole);
                    await user.RemoveRoleAsync(role).ConfigureAwait(false);
                    await $"{Context.User.Mention} just unmuted **{usertounmute.Username}**".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
            }
            catch
            {
                await $"Failed to remove the muted role from {usertounmute.Username}, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            }
        }

        [Command("prune"), Summary("Cleans set amount of messages.")]
        [RequireBotAndUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(short amount, IUser user = null)
        {
            if (amount < 0)
            {
                await $"{Context.User.Mention} Your amount `{amount}` is under 0.".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);

                DogStatsd.Increment("commands.errors", 1, 1, new[] { "unm-precon" });
                return;
            }
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (user == null)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync().ConfigureAwait(false);
                ITextChannel chan = (ITextChannel)Context.Channel;
                await chan.DeleteMessagesAsync(messages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                    {
                        await ":ok_hand: Done!".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync().ConfigureAwait(false);
                var usermessages = messages.Where(x => x.Author.Id == user.Id);
                usermessages = usermessages.Take(amount);
                ITextChannel chan = (ITextChannel)Context.Channel;
                await chan.DeleteMessagesAsync(usermessages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                    {
                        await ":ok_hand: Done!".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
        }

        #endregion Mute/Prune

        #region Ban/Kick

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
                    await user.KickAsync($"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await $"Successfully kicked: `{user.Username}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);

                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await user.KickAsync(reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await ($"Successfully kicked: `{user}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}\nReason: " + reason).QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);

                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await $"Couldn't kick {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
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
                    await Context.Guild.AddBanAsync(user, 7, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, 7, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await $"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
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
                    await Context.Guild.AddBanAsync(user, daystoprune, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, daystoprune, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await $"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await $"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            }
        }

        [Command("hackban"), Summary("Hackbans a set of userids Must be in this format hackban [id1] [id2] [id3]")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task HackBan(params string[] ids)
        {
            if (ids.Count() > 0)
            {
                foreach (var id in ids)
                    await Context.Guild.AddBanAsync(Convert.ToUInt64(id)).ConfigureAwait(false);

                await $"Banned IDs: {string.Join(", ", ids)}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
            else
            {
                await $"Couldn't parse list of ID's.".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
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
                await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
                await $"Successfully softbanned: `{user.Username}#{user.Discriminator}`".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
            }
            else
            {
                newreason += " - Reason: " + reason;
                await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
                await $"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
            }
        }

        #endregion Ban/Kick

        #region RoleManagement

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
                await $"```cs\n{lines}```".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("setjrole"), Summary("Allows a role to be auto assigned on userjoin"), RequireDatabase]
        public async Task AutoRole(IRole role = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;

            var gld = await Database.GetGuildAsync(guild);

            if (role == null)
            {
                if (gld.JoinRole != 0)
                {
                    gld.JoinRole = 0;
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    if ((await Database.GetGuildAsync(guild)).JoinRole == 0)
                    {
                        await $"Successfully removed the member join role".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                    }
                    else
                    {
                        await $"Error Removing Join Role, reason unknown.".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var roleidprev = gld.JoinRole;

                gld.JoinRole = role.Id;
                await Database.SaveChangesAsync().ConfigureAwait(false);

                if ((await Database.GetGuildAsync(guild)).JoinRole != roleidprev)
                {
                    await $"Successfully set **{role.Name}** as the member join role".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
                else
                {
                    await $"Error Changing Join Role, reason unknown.".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                }
            }
        }

        [Command("autorole"), Summary("Get's guilds current autorole"), RequireDatabase]
        public async Task AutoRole()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var jrole = (await Database.GetGuildAsync(Context.Guild)).JoinRole;

            if (jrole == 0)
            {
                await $"Currently, **{Context.Guild.Name}** has no auto role.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await $"**{Context.Guild.Name}**'s current auto role is `{Context.Guild.GetRole(jrole).Name}`".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("addassignablerole"), Summary("Adds a new self assignable role. Supported:cost=[cost] require-level=[level] require-role=[rolename/roleid/mention]")]
        [Alias("asar")]
        public async Task AddSARole(IRole role, [Remainder]GuildRoleConfig config = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (config == null)
                config = new GuildRoleConfig();

            var features = Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id);

            if (config.RequireLevel != 0 && !features.Experience)
            {
                await $"Enable Experience module first by using `{(await Database.GetGuildAsync(Context.Guild)).Prefix}guild-feature experience 1`".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            Database.IAmRoles.Add(new IAmRole
            {
                GuildId = Context.Guild.Id,
                LevelRequired = config.RequireLevel,
                Price = config.Cost,
                RequiredRoleId = config.RequiredRole != null ? config.RequiredRole.Id : 0,
                RoleId = role.Id
            });

            try
            {
                await Database.SaveChangesAsync().ConfigureAwait(false);
                await "".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);

                Log.Error("ASAR-CMD", ex.Message, ex);
            }
        }

        [Command("addlevelrole"), Summary("Adds a new self assignable role. Supported:cost=[cost] require-level=[level] require-role=[rolename/roleid/mention]")]
        [Alias("alr")]
        [Disabled]
        public async Task AddLRole(IRole role, [Remainder]GuildRoleConfig config)
        {
        }

        [Disabled]
        [Command("deleteasr"), Summary("Removes a Self Assignable Role from the list")]
        public async Task DeleteSelfRole([Remainder]IRole role)
        {
            //await DatabaseClient.DropCustomRole(role, false)
        }

        [Disabled]
        [Command("deletealr"), Summary("Removes a Level Grant Role from the list")]
        public async Task DeleteLevelRole([Remainder]IRole role)
        {
            //await DatabaseClient.DropCustomRole(role, true)
        }

        #endregion RoleManagement

        #region Prefix

        [Command("setprefix"), Summary("Sets the prefix, or resets on empty prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);
            if (prefix != null)
            {
                var oldprefix = gld.Prefix;

                gld.Prefix = prefix;

                await Database.SaveChangesAsync();

                if ((await Database.GetGuildAsync(Context.Guild)).Prefix != oldprefix)
                    await $"Successfully set `{prefix}` as the Guild's prefix".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                else
                    await $":thinking: It didn't change. Probably because it is the same as the current prefix.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                gld.Prefix = Configuration.Discord.Prefix;

                await Database.SaveChangesAsync();

                if ((await Database.GetGuildAsync(Context.Guild)).Prefix == Configuration.Discord.Prefix)
                    await $"Successfully reset the Guild's prefix".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                else
                    await $":thinking: It didn't change.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("resetprefix"), Summary("Resets prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetPrefix()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            if (gld != null)
            {
                gld.Prefix = MessageHandler.cmdConfig.Prefix;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"Reset the prefix back to `{MessageHandler.cmdConfig.Prefix}`".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await Database.InsertGuildAsync(Context.Guild, MessageHandler.cmdConfig.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon).ConfigureAwait(false);
            }
        }

        #endregion Prefix

        #region Welcome

        //Set Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome([Remainder]string welcome)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldmessage = gld.JoinMessage;

            gld.JoinChannel = Context.Channel.Id;
            gld.JoinMessage = welcome;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            if ((await Database.GetGuildAsync(Context.Guild)).JoinMessage != oldmessage)
                await $"Set Welcome message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
        }

        //Current Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldmessage = gld.JoinMessage;
            var oldchannel = gld.JoinChannel;

            gld.JoinChannel = channel.Id;
            gld.JoinMessage = welcome;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.JoinChannel != oldchannel && ngld.JoinMessage != oldmessage)
                await "Set Welcome message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
        }

        //Deletes
        [Command("unsetwelcome"), Summary("Clears the welcome message"), Alias("clearwelcome"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            gld.JoinChannel = 0;
            gld.JoinMessage = "";

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.JoinChannel == 0 && ngld.JoinMessage == "")
                await "Cleared Welcome message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
        }

        #endregion

        #region Leave

        //Set Channel
        [Command("setleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = channel.Id;
            gld.LeaveMessage = leave;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await "Set Leave message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        //Current Channel
        [Command("setleave"), Alias("clearleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave([Remainder]string leave)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = Context.Channel.Id;
            gld.LeaveMessage = leave;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await "Set Leave message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        //Deletes
        [Command("unsetleave"), Summary("Clears the leave message"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = 0;
            gld.LeaveMessage = "";

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await "Set Leave message!".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        #endregion

        #region Levels

        [Command("setlevelupmessage"), Summary("Sets the level up message, -u says the users name, -m mentions the user, -l shows the level they achieved"), RequireDatabase]
        [RequireRole(AccessLevel.ServerMod)]
        public async Task SetLevelUp([Remainder]string message)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldmessg = gld.LeaveMessage;

            gld.LevelUpMessage = message;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LevelUpMessage != oldmessg)
            {
                await "".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        [Command("levelchannel"), Summary("Sets the levelup channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureLevelChannel(IGuildChannel channel = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);
            var oldchan = gld.LevelUpChannel;

            if (channel == null)
            {
                gld.LevelUpChannel = channel.Id;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                var ngld = await Database.GetGuildAsync(Context.Guild);

                if (ngld.LevelUpChannel != oldchan)
                {
                    await "".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
            }
            else
            {
                gld.LevelUpChannel = 0;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                var ngld = await Database.GetGuildAsync(Context.Guild);

                if (ngld.LevelUpChannel != oldchan)
                {
                    await "".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
            }
        }

        [Command("levelnotification"), Summary("Sets the levelup notification")]
        [Alias("levelnotif")]
        public async Task ConfigureLevelNotif(LevelNotification level)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var old = gld.LevelNotification;

            gld.LevelNotification = level;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LevelNotification != old)
            {
                await "".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        #endregion

        #region CustomCommands

        [Command("addcommand"), Summary("Adds a custom command"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCustomCommand(string name, [Remainder]string content)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (name.IsWebsite())
            {
                await "Commands can't be a url/website".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
                return;
            }
            if (name.Split(' ').Length > 1)
            {
                await "Commands can't contain a space".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
                return;
            }
            else
            {
                var cmdsearch = CommandService.Search(Context, name);
                if (cmdsearch.Commands != null)
                {
                    await "The bot already has this command".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                }
                else
                {
                    var cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                    if (cmd != null)
                    {
                        await $"Custom command named `{name}` already exists, overwrite with new content? Y/N".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
                        var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        if (msg != null)
                        {
                            if (msg.Content.ToLower() == "y")
                            {
                                var c = content;

                                cmd.Content = content;

                                await Database.SaveChangesAsync().ConfigureAwait(false);

                                var cmd2 = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                                if (cmd2.Content != c)
                                {
                                    await "Updated the command.".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                                }
                                else
                                {
                                    await "Couldn't update the command".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            await "Reply timed out, not updating.".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);
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

                        cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                        if (cmd != null)
                            await "Added the command.".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                        else
                            await "Couldn't insert the command".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("deletecommand"), Summary("Deletes a custom command"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
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
                await "Are you sure? Y/N".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, null, null, 5).ConfigureAwait(false);

                var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                if (msg != null)
                {
                    if (msg.Content.ToLower() == "y")
                    {
                        Database.CustomCommands.Remove(Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.ToLower() == name.ToLower()));
                        await Database.SaveChangesAsync();

                        if (Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.ToLower() == name.ToLower()) == null)
                            await "Deleted the command.".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion
    }
}