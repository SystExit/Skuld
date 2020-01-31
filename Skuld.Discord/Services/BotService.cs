using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Utilities;
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
        private static List<ulong> UserIDs;
        public static int Users { get => UserIDs.Count(); }

        public static void ConfigureBot(DiscordSocketConfig config)
        {
            UserIDs = new List<ulong>();

            DiscordClient = new DiscordShardedClient(config);

            DiscordLogger.RegisterEvents();
        }

        public static void AddServices(IServiceProvider services)
            => Services = services;

        public static async Task StopBotAsync(string source)
        {
            DiscordLogger.UnRegisterEvents();

            await DiscordClient.SetStatusAsync(UserStatus.Offline);
            await DiscordClient.StopAsync();
            await DiscordClient.LogoutAsync();

            Log.Info(source, "Skuld is shutting down");
            DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

            Log.FlushNewLine();

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
                        foreach (var human in humans)
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