using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Database;
using Skuld.Discord.Handlers;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skuld.Discord.Services
{
    public static class BotService
    {
        public static DiscordShardedClient DiscordClient;
        public static CommandService CommandService { get => MessageHandler.CommandService; }
        public static IServiceProvider Services;
        static List<ulong> UserIDs;
        public static int Users { get => UserIDs.Count(); }

        public static void ConfigureBot(DiscordSocketConfig config)
        {
            UserIDs = new List<ulong>();

            DiscordClient = new DiscordShardedClient(config);

            DiscordLogger.RegisterEvents();
        }

        public static void AddServices(IServiceProvider services)
            => Services = services;

        public static void AddBotLister(BotListingClient botLister)
            => DiscordLogger.AddBotLister(botLister);

        public static async Task StopBotAsync(string source)
        {
            DiscordLogger.UnRegisterEvents();

            await DiscordClient.SetStatusAsync(UserStatus.Offline);
            await DiscordClient.StopAsync();
            await DiscordClient.LogoutAsync();

            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
            DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

            await GenericLogger.sw.WriteLineAsync("-------------------------------------------").ConfigureAwait(false);
            GenericLogger.sw.Close();

            await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void BackgroundTasks()
        {
            new Thread(
                async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await SendDataToDataDog().ConfigureAwait(false);
                }
            ).Start();
            new Thread(
                async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await FeedUsersAsync().ConfigureAwait(false);
                }
            ).Start();
            new Thread(
                async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await CleanDatabaseAsync();
                }
            ).Start();
        }

        private static Task SendDataToDataDog()
        {
            while (true)
            {
                DogStatsd.Gauge("shards.count", DiscordClient.Shards.Count);
                DogStatsd.Gauge("shards.connected", DiscordClient.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
                DogStatsd.Gauge("shards.disconnected", DiscordClient.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
                DogStatsd.Gauge("commands.count", CommandService.Commands.Count());
                if (DiscordClient.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                {
                    DogStatsd.Gauge("guilds.total", DiscordClient.Guilds.Count);
                }
                DogStatsd.Gauge("users.total", Users);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private static async Task CleanDatabaseAsync()
        {
            while (true)
            {
                if (DiscordClient.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                {
                    if (DatabaseClient.Enabled)
                    {
                        var res = await DatabaseClient.GetAllUserIDsAsync();
                        if (res.Successful)
                        {
                            var ids = res.Data as List<ulong>;
                            var sanitized = ids.Except(UserIDs).ToList();

                            foreach (var id in sanitized)
                            {
                                if (DiscordClient.GetUser(id) == null)
                                {
                                    await DatabaseClient.DropUserAsync(id);
                                }
                            }
                        }

                        var res2 = await DatabaseClient.GetAllGuildIDsAsync();
                        if (res2.Successful)
                        {
                            var ids = res2.Data as List<ulong>;
                            List<ulong> guildIDs = new List<ulong>();
                            foreach(var gld in DiscordClient.Guilds)
                            {
                                guildIDs.Add(gld.Id);
                            }
                            var sanitized = ids.Except(guildIDs).ToList();

                            foreach (var id in sanitized)
                            {
                                if (DiscordClient.GetGuild(id) == null)
                                {
                                    await DatabaseClient.DropGuildAsync(id);
                                }
                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(30));
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private static async Task FeedUsersAsync()
        {
            while (true)
            {
                if (DiscordClient.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                {
                    await DiscordClient.DownloadUsersAsync(DiscordClient.Guilds);
                    foreach (var gld in DiscordClient.Guilds)
                    {
                        var humans = gld.Users.Where(x => x.IsBot == false);
                        foreach(var human in humans)
                        {
                            if (!UserIDs.Contains(human.Id))
                                UserIDs.Add(human.Id);
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(15));
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
