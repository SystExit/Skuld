using Discord;
using Discord.Commands;
using Skuld.Commands;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Services;
using Skuld.Utilities.Discord;
using SysEx.Net;
using System;
using System.Threading.Tasks;

namespace Skuld.Modules
{
    [Group]
    public class Actions : SkuldBase<ShardedCommandContext>
    {
        public Random Random { get; set; }
        public SkuldConfig Configuration { get; set; }
        public DatabaseService Database { get; set; }
        public GenericLogger Logger { get; set; }
        public SysExClient SysExClient { get; set; }

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Slap).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"B-Baka.... {botguild.Mention} slapped {Context.User.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} slapped {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kill).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"{Context.User.Mention} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif").ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} killed {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Stab).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await SendAsync($"URUSAI!! {botguild.Mention} stabbed {user.Mention}", gif.URL).ConfigureAwait(false);
                return;
            }
            if (user.IsBot)
            {
                await SendAsync($"{Context.User.Mention} stabbed {user.Mention}", gif.URL).ConfigureAwait(false);
                return;
            }
            if (await Database.CheckConnectionAsync())
            {
                uint dhp = (uint)Random.Next(0, 100);

                var usr = await Database.GetUserAsync(user.Id).ConfigureAwait(false);

                if (usr != null)
                {
                    if (dhp < usr.HP)
                    {
                        usr.HP -= dhp;

                        await Database.UpdateUserAsync(usr);

                        await SendAsync($"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
                    }
                    else
                    {
                        usr.HP = 0;
                        await Database.UpdateUserAsync(usr);

                        await SendAsync($"{Context.User.Mention} just stabbed {user.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await SendAsync($"{Context.User.Mention} stabbed {user.Mention}", gif.URL).ConfigureAwait(false);
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
                await SendAsync($"{botguild.Mention} hugs {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} just hugged {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Punch).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"URUSAI!! {botguild.Mention} just punched {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} just punched {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("shrug"), Summary("Shrugs")]
        public async Task Shrug()
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Shrug).ConfigureAwait(false);

            await SendAsync($"{Context.User.Mention} shrugs.", gif.URL).ConfigureAwait(false);
        }

        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Adore).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} adores {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} adores {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kiss).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} just kissed {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} just kissed {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser user)
        {
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Grope).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                await SendAsync($"{botguild.Mention} just groped {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{Context.User.Mention} just groped {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("pat"), Summary("Pat a user"), Alias("pet", "headpat")]
        public async Task Pat([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Pet).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await SendAsync($"{botguild.Mention} just headpatted {user.Mention}", gif.URL).ConfigureAwait(false);
                return;
            }
            if (user.IsBot)
            {
                await SendAsync($"{Context.User.Mention} just headpatted {user.Mention}", gif.URL).ConfigureAwait(false);
                return;
            }
            if (await Database.CheckConnectionAsync())
            {
                var cusr = await Database.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                if (cusr != null)
                {
                    cusr.Pats += 1;

                    await Database.UpdateUserAsync(cusr).ConfigureAwait(false);

                    var gusr = await Database.GetUserAsync(user.Id).ConfigureAwait(false);
                    if (gusr != null)
                    {
                        gusr.Patted += 1;

                        await Database.UpdateUserAsync(gusr).ConfigureAwait(false);

                        await SendAsync($"{Context.User.Mention} just headpatted {user.Mention}, they've been petted {gusr.Patted} time(s)!", gif.URL).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await SendAsync($"{Context.User.Mention} just headpatted {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("glare"), Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser user)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Glare).ConfigureAwait(false);

            if (user == Context.User as IGuildUser)
            {
                var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
                await SendAsync($"{botguild.Mention} glares at {user.Mention}", gif.URL).ConfigureAwait(false);
            }
            if (user.IsBot)
            {
                await SendAsync($"{Context.User.Mention} glares at {user.Mention}", gif.URL).ConfigureAwait(false);
            }

            if (await Database.CheckConnectionAsync())
            {
                var usr = await Database.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                if (usr != null)
                {
                    usr.Glares += 1;

                    await Database.UpdateUserAsync(usr).ConfigureAwait(false);

                    var usr2 = await Database.GetUserAsync(user.Id).ConfigureAwait(false);

                    if (usr2 != null)
                    {
                        usr2.GlaredAt += 1;

                        await Database.UpdateUserAsync(usr2).ConfigureAwait(false);

                        await SendAsync($"{Context.User.Mention} glares at {user.Mention}, they've been glared at {usr2.GlaredAt} time(s)!", gif.URL).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await SendAsync($"{Context.User.Mention} glares at {user.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        private async Task SendAsync(string message, string image)
            => await ReplyAsync(Context.Channel, new EmbedBuilder { Description = message, Color = EmbedUtils.RandomColor(), ImageUrl = image }.Build());
    }
}