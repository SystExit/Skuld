using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Core.Extensions;
using Skuld.Models.Database;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Skuld.Core.Services;

namespace Skuld.Services
{
    public class ExperienceService
    {
        private readonly DiscordShardedClient client;
        private DatabaseService database;
        private GenericLogger logger;

        public ExperienceService(DiscordShardedClient cli, DatabaseService db, GenericLogger log) //inherits from depinjection
        {
            client = cli;
            database = db;
            logger = log;
            client.MessageReceived += Client_MessageReceived;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            var context = new ShardedCommandContext(client, message);

            if (!MessageTools.IsEnabledChannel(context.Guild.GetUser(context.User.Id), (ITextChannel)context.Channel)) { return; }

            try
            {
                SkuldGuild sguild = null;

                if (database.CanConnect)
                {
                    var usr = await database.GetUserAsync(context.User.Id);
                    if (usr == null) { await database.InsertUserAsync(context.User).ConfigureAwait(false); usr = await database.GetUserAsync(context.User.Id); }
                    if (usr.Banned) return;

                    if (context.Guild != null)
                    {
                        sguild = await database.GetGuildAsync(context.Guild.Id).ConfigureAwait(false);
                        if (sguild == null)
                        {
                            await database.InsertGuildAsync(context.Guild).ContinueWith(async x =>
                            {
                                sguild = await database.GetGuildAsync(context.Guild.Id).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            if (sguild.GuildSettings.Features.Experience)
                            {
                                var luserexperience = await database.GetUserExperienceAsync(context.User);

                                if (luserexperience is UserExperience)
                                {
                                    var luxp = (UserExperience)luserexperience;

                                    var amount = (ulong)HostService.Services.GetRequiredService<Random>().Next(0, 25);

                                    if (luxp.GuildExperiences.Count() != 0)
                                    {
                                        var gld = luxp.GuildExperiences.First(x => x.GuildID == context.Guild.Id);
                                        if (gld != null)
                                        {
                                            if (gld.LastGranted < (DateTime.UtcNow.ToEpoch() - 60))
                                            {

                                                var xptonextlevel = GetXPRequirement(gld.Level + 1, 1.618); //get next level xp requirement based on phi

                                                if ((gld.XP + amount) >= xptonextlevel) //if over or equal to next level requirement, update accordingly
                                                {
                                                    gld.XP = 0;
                                                    gld.TotalXP += amount;
                                                    gld.Level++;
                                                    gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                                    await context.Channel.SendMessageAsync($"Congratulations {context.User.Mention}!! You're now level **{gld.Level}**");
                                                    await logger.AddToLogsAsync(new Core.Models.LogMessage("MessageService", "User levelled up", LogSeverity.Info));
                                                    DogStatsd.Increment("user.levels.levelup");
                                                    DogStatsd.Increment("user.levels.processed");
                                                }
                                                else //otherwise append current status
                                                {
                                                    gld.XP += amount;
                                                    gld.TotalXP += amount;
                                                    gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                                }
                                                await database.UpdateGuildExperienceAsync(context.User, gld, context.Guild);
                                                DogStatsd.Increment("user.levels.processed");
                                            }
                                        }
                                        else
                                        {
                                            gld = new GuildExperience();
                                            gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                            gld.XP = gld.TotalXP = amount;
                                            await database.InsertGuildExperienceAsync(context.User, context.Guild, gld);
                                            DogStatsd.Increment("user.levels.processed");
                                        }
                                    }
                                    else
                                    {
                                        var gld = new GuildExperience();
                                        gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                        gld.XP = gld.TotalXP = amount;
                                        await database.InsertGuildExperienceAsync(context.User, context.Guild, gld);
                                        DogStatsd.Increment("user.levels.processed");
                                    }
                                }
                                else
                                {
                                    var gld = new GuildExperience();
                                    gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                    gld.XP = gld.TotalXP = (ulong)HostService.Services.GetRequiredService<Random>().Next(0, 25);
                                    await database.InsertGuildExperienceAsync(context.User, context.Guild, gld);
                                    DogStatsd.Increment("user.levels.processed");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("CmdDisp", ex.Message, LogSeverity.Error, ex));
            }
        }
        private ulong GetXPRequirement(ulong level, double growthmod)
            => (ulong)((level * 50) * (level * growthmod));
    }
}