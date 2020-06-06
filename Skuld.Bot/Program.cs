using Discord;
using Discord.WebSocket;
using Sentry;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Bot;
using Skuld.Services.Bot.Discord;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordNetCommands = Discord.Commands;

namespace Skuld.Bot
{
    public static class Program
    {
        private static void Main(string[] args)
            => CreateAsync(args).GetAwaiter().GetResult();

        public static async Task CreateAsync(string[] args = null)
        {
            SkuldConfig Configuration = null;

            if (!Directory.Exists(SkuldAppContext.LogDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.LogDirectory);
            }
            if (!Directory.Exists(SkuldAppContext.StorageDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.StorageDirectory);
            }
            if (!Directory.Exists(SkuldAppContext.FontDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.FontDirectory);
            }

            try
            {
                var database = new SkuldDbContextFactory().CreateDbContext();

                await database.ApplyPendingMigrations().ConfigureAwait(false);

                if (!database.Configurations.Any() ||
                    args.Contains("--newconf") ||
                    args.Contains("-nc"))
                {
                    var conf = new SkuldConfig();
                    database.Configurations.Add(conf);
                    await database.SaveChangesAsync().ConfigureAwait(false);
                    Log.Verbose("HostService", $"Created new configuration with Id: {conf.Id}", null);
                }

                var configId = SkuldAppContext.GetEnvVar(SkuldAppContext.ConfigEnvVar);

                var c = database.Configurations.FirstOrDefault(x => x.Id == configId);

                Configuration = c ?? database.Configurations.FirstOrDefault();

                SkuldAppContext.SetConfigurationId(Configuration.Id);
            }
            catch (Exception ex)
            {
                Log.Critical("HostService", ex.Message, null, ex);
            }

            await BotService.ConfigureBotAsync(
                Configuration,
                new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    DefaultRetryMode = RetryMode.AlwaysRetry,
                    LogLevel = LogSeverity.Verbose,
                    TotalShards = Configuration.Shards
                },
                new DiscordNetCommands.CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = DiscordNetCommands.RunMode.Async,
                    LogLevel = LogSeverity.Verbose,
                    IgnoreExtraArgs = true
                },
                new MessageServiceConfig
                {
                    Prefix = Configuration.Prefix,
                    AltPrefix = Configuration.AltPrefix
                }
            ).ConfigureAwait(false);

            Log.Info("HostService", "Loaded Skuld v" + SkuldAppContext.Skuld.Key.Version);

            var sentryKey = Environment.GetEnvironmentVariable(SkuldAppContext.SentryIOEnvVar);

            if(sentryKey != null)
            {
                using (SentrySdk.Init(sentryKey))
                {
                    await BotService.StartBotAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await BotService.StartBotAsync().ConfigureAwait(false);
            }

            await Task.Delay(-1).ConfigureAwait(false);

            BotService.WebSocket.ShutdownServer();

            Environment.Exit(0);
        }
    }
}