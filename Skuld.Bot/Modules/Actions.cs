using Discord;
using Discord.Commands;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord;
using SysEx.Net;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Actions : SkuldBase<SkuldCommandContext>
    {
        public Random Random { get; set; }
        public SkuldConfig Configuration { get; set; }
        public SysExClient SysExClient { get; set; }

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Slap).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"B-Baka.... {botguild.Mention} slapped {Context.User.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} slapped {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kill).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(new Uri("http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif"), embeddesc: $"{Context.User.Mention} killed themself")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} killed {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Stab).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"URUSAI!! {botguild.Mention} stabbed {user.Mention}")).ConfigureAwait(false);
                return;
            }
            if (user.IsBot)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} stabbed {user.Mention}")).ConfigureAwait(false);
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

                        await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left")).ConfigureAwait(false);
                    }
                    else
                    {
                        usr.HP = 0;
                        await DatabaseClient.UpdateUserAsync(usr);

                        await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left")).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} stabbed {user.Mention}")).ConfigureAwait(false);
                return;
            }
        }

        [Command("hug"), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Hug).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} hugs {user.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just hugged {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Punch).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"URUSAI!! {botguild.Mention} just punched {user.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just punched {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("shrug"), Summary("Shrugs")]
        public async Task Shrug()
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Shrug).ConfigureAwait(false);

            await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} shrugs.")).ConfigureAwait(false);
        }

        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Adore).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} adores {user.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} adores {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kiss).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} just kissed {user.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just kissed {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Grope).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} just groped {user.Mention}")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just groped {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("pat"), Summary("Pat a user"), Alias("pet", "headpat")]
        public async Task Pat([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Pet).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} just headpatted {user.Mention}")).ConfigureAwait(false);
                return;
            }
            if (user.IsBot)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}")).ConfigureAwait(false);
                return;
            }
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var usrResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                if (usrResp.Successful)
                {
                    var usr = usrResp.Data as SkuldUser;
                    usr.Pats += 1;

                    await DatabaseClient.UpdateUserAsync(usr).ConfigureAwait(false);

                    var gusrResp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                    if (gusrResp.Successful)
                    {
                        var gusr = gusrResp.Data as SkuldUser;
                        gusr.Patted += 1;

                        await DatabaseClient.UpdateUserAsync(gusr).ConfigureAwait(false);

                        await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}, they've been petted {gusr.Patted} time(s)!")).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} just headpatted {user.Mention}")).ConfigureAwait(false);
            }
        }

        [Command("glare"), Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Glare).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} glares at {user.Mention}")).ConfigureAwait(false);
            }
            if (user.IsBot)
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} glares at {user.Mention}")).ConfigureAwait(false);
            }

            if (await DatabaseClient.CheckConnectionAsync())
            {
                var usrResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                if (usrResp.Successful)
                {
                    var usr = usrResp.Data as SkuldUser;
                    usr.Glares += 1;

                    await DatabaseClient.UpdateUserAsync(usr).ConfigureAwait(false);

                    var usr2Resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);

                    if (usr2Resp.Successful)
                    {
                        var usr2 = usr2Resp.Data as SkuldUser;
                        usr2.GlaredAt += 1;

                        await DatabaseClient.UpdateUserAsync(usr2).ConfigureAwait(false);

                        await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} glares at {user.Mention}, they've been glared at {usr2.GlaredAt} time(s)!")).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await ReplyAsync(Context.Channel, EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} glares at {user.Mention}")).ConfigureAwait(false);
            }
        }
    }
}