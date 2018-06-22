using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using SysEx.Net;
using Skuld.Services;
using Skuld.Utilities;
using Skuld.Extensions;

namespace Skuld.Modules
{
    [Group]
    public class Actions : ModuleBase<ShardedCommandContext>
    {
        public Random Random { get; set; }
        public DatabaseService Database { get; set; }
        public SysExClient SysExClient { get; set; }

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Slap).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"B-Baka.... {botguild.Mention} slapped {contuser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} slapped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]IGuildUser guilduser)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kill).ConfigureAwait(false);

            var contuser = Context.User as IGuildUser;
            if (contuser == guilduser)
            {
                await SendAsync($"{contuser.Mention} killed themself", "http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif").ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} killed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Stab).ConfigureAwait(false);

            if (contuser == guilduser)
            {
                await SendAsync($"URUSAI!! {botguild.Mention} stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else if (guilduser.IsBot)
            {
                await SendAsync($"{contuser.Mention} stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                if (Database.CanConnect)
                {
                    uint dhp = (uint)Random.Next(0, 100);

                    var usr = await Database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

                    if (usr == null)
                    {
                        await InsertUser(guilduser).ConfigureAwait(false);
                    }
                    else
                    {
                        if (dhp < usr.HP)
                        {
                            usr.HP -= dhp;

                            await Database.UpdateUserAsync(usr);

                            await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
                        }
                        else
                        {
                            usr.HP = 0;
                            await Database.UpdateUserAsync(usr);

                            await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention} for {dhp} HP, they now have {usr.HP} HP left", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await SendAsync($"{contuser.Mention} just stabbed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
                }
            }
        }

        [Command("hug"), Summary("hugs a user")]
        public async Task Hug([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Hug).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"{botguild.Mention} hugs {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} just hugged {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Punch).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"URUSAI!! {botguild.Mention} just punched {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} just punched {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("shrug"), Summary("Shrugs")]
        public async Task Shrug()
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Shrug).ConfigureAwait(false);

            await SendAsync($"{Context.User.Mention} shrugs.", gif.URL).ConfigureAwait(false);
        }

        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Adore).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} adores {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} adores {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kiss).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"I-it's not like I like you or anything... {botguild.Mention} just kissed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} just kissed {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Grope).ConfigureAwait(false);

            if (guilduser == contuser)
            {
                await SendAsync($"{botguild.Mention} just groped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                await SendAsync($"{contuser.Mention} just groped {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
        }

        [Command("pat"), Summary("Pats a user's head"), Alias("pet", "headpat")]
        public async Task Pat([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Pet).ConfigureAwait(false);

            if (contuser == guilduser)
            {
                await SendAsync($"{botguild.Mention} just headpatted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else if (guilduser.IsBot)
            {
                await SendAsync($"{contuser.Mention} just headpatted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                if (Database.CanConnect)
                {
                    var cusr = await Database.GetUserAsync(Context.User.Id).ConfigureAwait(false);

                    if (cusr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        cusr.Pets += 1;

                        await Database.UpdateUserAsync(cusr).ConfigureAwait(false);

                        var gusr = await Database.GetUserAsync(guilduser.Id).ConfigureAwait(false);
                        if (gusr == null)
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            gusr.Petted += 1;

                            await Database.UpdateUserAsync(gusr).ConfigureAwait(false);

                            await SendAsync($"{contuser.Mention} just headpatted {guilduser.Mention}, they've been petted {gusr.Petted} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await SendAsync($"{contuser.Mention} just headpatted {guilduser.Mention}", gif.URL).ConfigureAwait(false);
                }
            }
        }

        [Command("glare"), Summary("Glares at a user")]
        public async Task Glare([Remainder]IGuildUser guilduser)
        {
            var contuser = Context.User as IGuildUser;
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Glare).ConfigureAwait(false);

            if (contuser == guilduser)
            {
                await SendAsync($"{botguild.Mention} glares at {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else if (guilduser.IsBot)
            {
                await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}", gif.URL).ConfigureAwait(false);
            }
            else
            {
                if (Database.CanConnect)
                {
                    var usr = await Database.GetUserAsync(contuser.Id).ConfigureAwait(false);

                    if (usr == null)
                    {
                        await InsertUser(contuser).ConfigureAwait(false);
                    }
                    else
                    {
                        usr.Glares += 1;

                        await Database.UpdateUserAsync(usr).ConfigureAwait(false);

                        var usr2 = await Database.GetUserAsync(guilduser.Id).ConfigureAwait(false);

                        if (usr2 == null)
                        {
                            await InsertUser(guilduser).ConfigureAwait(false);
                        }
                        else
                        {
                            usr2.GlaredAt += 1;

                            await Database.UpdateUserAsync(usr2).ConfigureAwait(false);

                            await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}, they've been glared at {usr2.GlaredAt} time(s)!", gif.URL).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await SendAsync($"{contuser.Mention} glares at {guilduser.Mention}", gif.URL).ConfigureAwait(false);
                }
            }
        }

        private async Task InsertUser(IUser user)
        {
            await Database.InsertUserAsync(user);
        }
        private async Task SendAsync(string message, string image)
            => await Context.Channel.ReplyAsync(new EmbedBuilder { Description = message, Color = EmbedUtils.RandomColor(), ImageUrl = image }.Build());
    }
}
