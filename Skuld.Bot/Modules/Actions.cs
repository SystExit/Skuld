using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Database.Extensions;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Utilities;
using SysEx.Net;
using SysEx.Net.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Actions : InteractiveBase<SkuldCommandContext>
    {
        public Random Random { get; set; }
        public SkuldConfig Configuration { get; set; }
        public SysExClient SysExClient { get; set; }

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Slap).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"B-Baka.... {botguild.Mention} slapped {Context.User.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} slapped {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-SLAP", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "slap").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kill).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(new Uri("http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif"), embeddesc: $"{Context.User.Mention} killed themself").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} killed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-KILL", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "kill").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Stab).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"URUSAI!! {botguild.Mention} stabbed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        return;
                    }
                    if (user.IsBot)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} stabbed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        return;
                    }
                    if (await DatabaseClient.CheckConnectionAsync())
                    {
                        uint dhp = (uint)Random.Next(0, 100);

                        var usrResp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);

                        if (usrResp.Successful)
                        {
                            var usr = usrResp.Data as SkuldUser;
                            if (dhp < usr.HP)
                            {
                                usr.HP -= dhp;

                                await DatabaseClient.UpdateUserAsync(usr);

                                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                            }
                            else
                            {
                                usr.HP = 0;
                                await DatabaseClient.UpdateUserAsync(usr);

                                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                            }
                        }
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} stabbed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-STAB", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "stab").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("hug"), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Hug).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} hugs {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just hugged {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-HUG", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "hug").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Punch).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"URUSAI!! {botguild.Mention} just punched {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just punched {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-PUNCH", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "punch").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("shrug"), Summary("Shrugs")]
        public async Task Shrug()
        {
            try
            {
                var gif = await SysExClient.GetWeebActionGifAsync(GifType.Shrug).ConfigureAwait(false);

                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} shrugs.").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-SHRUG", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Adore).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} adores {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} adores {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-ADORE", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "adore").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kiss).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} just kissed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just kissed {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-KISS", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "kiss").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Grope).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} just groped {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just groped {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-GROPE", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "grope").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("pat"), Summary("Pat a user"), Alias("pet", "headpat")]
        public async Task Pat([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                try
                {
                    var gif = await SysExClient.GetWeebActionGifAsync(GifType.Pet).ConfigureAwait(false);

                    if (user == Context.User as IGuildUser)
                    {
                        var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} just headpatted {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        return;
                    }
                    if (user.IsBot)
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        return;
                    }
                    if (await DatabaseClient.CheckConnectionAsync())
                    {
                        var usrResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                        if (usrResp.Successful)
                        {
                            var usr = usrResp.Data as SkuldUser;

                            var gusrResp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                            if (gusrResp.Successful)
                            {
                                var gusr = gusrResp.Data as SkuldUser;
                                if (!gusr.RecurringBlock || (!(gusr.Patted).IsRecurring(2) && gusr.RecurringBlock))
                                {
                                    usr.Pats += 1;
                                    await DatabaseClient.UpdateUserAsync(usr).ConfigureAwait(false);

                                    gusr.Patted += 1;

                                    await DatabaseClient.UpdateUserAsync(gusr).ConfigureAwait(false);

                                    await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}, they've been petted {gusr.Patted} time(s)!").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                                }
                                else
                                {
                                    await EmbedUtils.EmbedImage(new Uri("https://cdn.discordapp.com/attachments/306137824023805953/552393620448215050/image0-7.jpg"), "", $"{user.Mention} doged your pet, pet someone else. (They've blocked users from petting them)").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                                }
                            }
                            else
                            {
                                await DatabaseClient.InsertUserAsync(user);
                                gusrResp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                                if (gusrResp.Successful)
                                {
                                    var gusr = gusrResp.Data as SkuldUser;
                                    gusr.Patted += 1;
                                    usr.Pats += 1;

                                    await DatabaseClient.UpdateUserAsync(usr).ConfigureAwait(false);

                                    await DatabaseClient.UpdateUserAsync(gusr).ConfigureAwait(false);

                                    await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}, they've been petted {gusr.Patted} time(s)!").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                                }
                            }
                        }
                        else
                        {
                            await DatabaseClient.InsertUserAsync(Context.User);
                            await Pat(user);
                            return;
                        }
                    }
                    else
                    {
                        await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                catch (Exception ex)
                {
                    await ex.Message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, null, ex);
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-PAT", ex.Message, LogSeverity.Error, ex));
                }
            }
            else
            {
                await EmbedUtils.EmbedImage(new Uri("https://cdn.discordapp.com/attachments/306137824023805953/552393620448215050/image0-7.jpg"), "", GetBlockedMessage(Context.User, user, "pat")).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("glare"), Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser user)
        {
            if (await CanPerformActions(await MessageTools.GetUserOrInsertAsync(user), Context.DBUser))
            {
                var gif = await SysExClient.GetWeebActionGifAsync(GifType.Glare).ConfigureAwait(false);

                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} glares at {user.Mention}").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                await GetBlockedMessage(Context.User, user, "glare").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        private string GetBlockedMessage(IUser blocked, IUser blockee, string command)
            => $"{blocked.Mention} the user {blockee.Mention} dodged your {command}";

        public static async Task<bool> CanPerformActions(SkuldUser blocker, SkuldUser blockee)
        {
            var res = await blockee.IsBlockedFromPerformingActions(blocker.ID);

            if (res.Successful)
            {
                return !Convert.ToBoolean(res.Data);
            }

            return true;
        }
    }
}
