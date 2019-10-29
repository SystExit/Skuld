using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Bot.Services;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using SysEx.Net;
using SysEx.Net.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Actions : InteractiveBase<ShardedCommandContext>
    {
        public Random Random { get; set; }
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        public SysExClient SysExClient { get; set; }

        [Command("slap"), Summary("Slap a user")]
        public async Task Slap([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Slap).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"B-Baka.... {botguild.Mention} slapped {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} slaps {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("kill"), Summary("Kills a user")]
        public async Task Kill([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kill).ConfigureAwait(false);

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(new Uri("http://i.giphy.com/l2JeiuwmhZlkrVOkU.gif"), embeddesc: $"{Context.User.Mention} killed themself").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} kills {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("stab"), Summary("Stabs a user")]
        public async Task Stab([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Stab).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"URUSAI!! {botguild.Mention} stabs {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} stabs {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("hug"), Summary("hugs a user")]
        public async Task Hug([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Hug).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} hugs {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} hugs {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("punch"), Summary("Punch a user")]
        public async Task Punch([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Punch).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} punches {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} punches {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("shrug"), Summary("Shrugs")]
        public async Task Shrug()
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Shrug).ConfigureAwait(false);
            await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} shrugs.").QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("adore"), Summary("Adore a user")]
        public async Task Adore([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Adore).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} adores {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} adores {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("kiss"), Summary("Kiss a user")]
        public async Task Kiss([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Kiss).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"I-it's not like I like you or anything... {botguild.Mention} kisses {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} kisses {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("grope"), Summary("Grope a user")]
        public async Task Grope([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Grope).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} gropes {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} gropes {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("pat"), Summary("Pat a user"), Alias("pet", "headpat")]
        public async Task Pat([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Pet).ConfigureAwait(false);

            if (Context.Message.MentionedUsers.Count > 0)
            {
                using SkuldDatabaseContext Database = new SkuldDbContextFactory().CreateDbContext(null);
                var initiator = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

                StringBuilder message = new StringBuilder();

                var msg = target;

                foreach (var usr in Context.Message.MentionedUsers)
                {
                    if (usr.IsBot || usr.IsWebhook || usr.Discriminator == "0000")
                        continue;

                    var uzr = await Database.GetUserAsync(usr).ConfigureAwait(false);

                    if (!(uzr.RecurringBlock && uzr.Patted.IsRecurring(2)))
                    {
                        uzr.Patted += 1;
                        initiator.Pats += 1;

                        message.Append(usr.Mention + " ");

                        msg = msg.Replace($"<@{usr.Id}> ", "");
                        msg = msg.Replace($"<@{usr.Id}>", "");
                        msg = msg.Replace($"<@!{usr.Id}> ", "");
                        msg = msg.Replace($"<@!{usr.Id}>", "");
                    }
                }

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} pats {message} {msg}").QueueMessageAsync(Context).ConfigureAwait(false);

                return;
            }

            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} pats {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} pats {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("glare"), Summary("Glares at a user")]
        public async Task Glare([Remainder]string target = null)
        {
            var gif = await SysExClient.GetWeebActionGifAsync(GifType.Glare).ConfigureAwait(false);
            var botguild = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;

            if (target == null || (MentionUtils.TryParseUser(target, out ulong userId) && userId == Context.User.Id))
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{botguild.Mention} glares at {Context.User.Mention}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedUtils.EmbedImage(gif.ToUri(), embeddesc: $"{Context.User.Mention} glares at {target}").QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }
    }
}